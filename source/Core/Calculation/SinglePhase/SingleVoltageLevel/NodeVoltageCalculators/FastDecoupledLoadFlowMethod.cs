using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        private readonly IIterativeSolver<double> _solver;
        private readonly IPreconditioner<double> _preconditioner;

        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations, bool iterativeSolver)
            : base(targetPrecision, maximumIterations)
        {
            _solver = new BiCgStab();
            _preconditioner = new ILU0Preconditioner();
            IterativeSolver = iterativeSolver;
        }

        public bool IterativeSolver { get; private set; }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<double> pvBusVoltages, double residualImprovementFactor, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, IReadOnlyDictionary<int, int> pvBusToMatrixIndex, IReadOnlyDictionary<int, int> busToMatrixIndex)
        {
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(busToMatrixIndex.Count);
            var realPowerMaximumError = powersRealError.Select(Math.Abs).Max();
            var imaginaryPowerMaximumError = pqBusToMatrixIndex.Count > 0 ? powersImaginaryError.Select(Math.Abs).Max() : 0;
            var powerMaximumError = Math.Max(realPowerMaximumError, imaginaryPowerMaximumError);
            Vector<double> angleChange = new SparseVector(busToMatrixIndex.Count);
            Matrix<double> changeMatrixRealPowerByAngle;
            Matrix<double> changeMatrixImaginaryPowerByAmplitude;
            CalculateChangeMatrices(admittances, voltages, constantCurrents, busToMatrixIndex, pqBusToMatrixIndex, out changeMatrixRealPowerByAngle, out changeMatrixImaginaryPowerByAmplitude);

            if (realPowerMaximumError > TargetPrecision)
            {
                var powersRealErrorVector = new DenseVector(powersRealError.ToArray());

                angleChange = SolveLinearEquationSystem(changeMatrixRealPowerByAngle, powersRealErrorVector, residualImprovementFactor, powerMaximumError);
            }

            if (pqBusToMatrixIndex.Count > 0)
            {
                Vector<double> amplitudeChange = new SparseVector(pqBusToMatrixIndex.Count);

                if (imaginaryPowerMaximumError > TargetPrecision)
                {
                    var powersImaginaryErrorVector = new DenseVector(powersImaginaryError.ToArray());
                    amplitudeChange = SolveLinearEquationSystem(changeMatrixImaginaryPowerByAmplitude,
                        powersImaginaryErrorVector, residualImprovementFactor, powerMaximumError);
                }

                foreach (var bus in pqBusToMatrixIndex.Select(x => x.Key))
                    improvedVoltages[bus] =
                        Complex.FromPolarCoordinates(
                            voltages[bus].Magnitude + amplitudeChange[pqBusToMatrixIndex[bus]],
                            voltages[bus].Phase + angleChange[bus]);
            }

            foreach (var bus in pvBusToMatrixIndex)
                improvedVoltages[bus.Key] = Complex.FromPolarCoordinates(pvBusVoltages[bus.Value], voltages[bus.Key].Phase + angleChange[bus.Key]);

            return improvedVoltages;
        }

        private Vector<double> SolveLinearEquationSystem(Matrix<double> changeMatrix, Vector<double> errorVector, double residualImprovementFactor, double powerMaximumError)
        {
            var residualStopCriterion = new ResidualStopCriterion<double>(powerMaximumError * residualImprovementFactor);
            var iterationStopCriterion =
                new IterationCountStopCriterion<double>(Math.Max(changeMatrix.ColumnCount, 20));
            var stopCriterion =
                new DelegateStopCriterion<double>(
                    (i, vector, arg3, arg4) =>
                        residualStopCriterion.DetermineStatus(i, vector, arg3, arg4) == IterationStatus.Continue &&
                        iterationStopCriterion.DetermineStatus(i, vector, arg3, arg4) == IterationStatus.Continue
                            ? IterationStatus.Continue
                            : IterationStatus.StoppedWithoutConvergence);
            var iterator = new Iterator<double>(stopCriterion);
            Vector<double> angleChange = new DenseVector(changeMatrix.RowCount);

            if (IterativeSolver)
                _solver.Solve(changeMatrix, errorVector, angleChange, iterator, _preconditioner);
            else
            {
                var factorization = changeMatrix.LU();
                angleChange = factorization.Solve(errorVector);
            }

            return angleChange;
        }

        private static void CalculateChangeMatrices(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages,
            IList<Complex> constantCurrents, IReadOnlyDictionary<int, int> busToMatrixIndex, IReadOnlyDictionary<int, int> pqBusToMatrixIndex,
            out Matrix<double> changeMatrixRealPowerByAngle, out Matrix<double> changeMatrixImaginaryPowerByAmplitude)
        {
            changeMatrixRealPowerByAngle = new SparseMatrix(busToMatrixIndex.Count);
            changeMatrixImaginaryPowerByAmplitude = pqBusToMatrixIndex.Count > 0 ? new SparseMatrix(pqBusToMatrixIndex.Count) : null;

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                var admittance = entry.Item3;
                FillChangeMatrixImaginaryPowerByAmplitude(changeMatrixImaginaryPowerByAmplitude, voltages, pqBusToMatrixIndex, busRow, busColumn, admittance);
                FillChangeMatrixRealPowerByAngle(changeMatrixRealPowerByAngle, voltages, busToMatrixIndex, busRow, busColumn, admittance);
            }

            foreach (var bus in busToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var voltage = voltages[busIndex];
                var current = constantCurrents[busIndex];
                changeMatrixRealPowerByAngle[matrixIndex, matrixIndex] +=
                    voltage.Magnitude * current.Magnitude * Math.Sin(voltage.Phase - current.Phase);
            }

            foreach (var bus in pqBusToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var current = constantCurrents[busIndex];
                var voltage = voltages[busIndex];
                changeMatrixImaginaryPowerByAmplitude[matrixIndex, matrixIndex] +=
                    current.Magnitude * Math.Sin(current.Phase - voltage.Phase);
            }
        }

        private static void FillChangeMatrixRealPowerByAngle(Matrix<double> changeMatrixRealPowerByAngle, IList<Complex> voltages, IReadOnlyDictionary<int, int> busToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            var realPowerByAngleRow = busToMatrixIndex[busRow];
            var realPowerByAngleColumn = busToMatrixIndex[busColumn];

            if (realPowerByAngleRow == realPowerByAngleColumn) 
                return;

            var voltageRow = voltages[busRow];
            var voltageColumn = voltages[busColumn];
            changeMatrixRealPowerByAngle[realPowerByAngleRow, realPowerByAngleColumn] =
                admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
            changeMatrixRealPowerByAngle[realPowerByAngleRow, realPowerByAngleRow] +=
                admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        private static void FillChangeMatrixImaginaryPowerByAmplitude(Matrix<double> changeMatrixImaginaryPowerByAmplitude, IList<Complex> voltages, IReadOnlyDictionary<int, int> pqBusToMatrixIndex, int busRow, int busColumn, Complex admittance)
        {
            int imaginaryPowerByAmplitudeRow;

            if (!pqBusToMatrixIndex.TryGetValue(busRow, out imaginaryPowerByAmplitudeRow)) 
                return;

            var voltageRow = voltages[busRow];
            int imaginaryPowerByAmplitudeColumn;
            if (!pqBusToMatrixIndex.TryGetValue(busColumn, out imaginaryPowerByAmplitudeColumn))
            {
                Complex voltageColumn1 = voltages[busColumn];
                changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow] +=
                    admittance.Magnitude * voltageColumn1.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn1.Phase);
                return;
            }

            if (imaginaryPowerByAmplitudeRow == imaginaryPowerByAmplitudeColumn)
            {
                changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeColumn, imaginaryPowerByAmplitudeColumn] -=
                    2 * admittance.Magnitude * voltageRow.Magnitude * Math.Sin(admittance.Phase);
                return;
            }

            var voltageColumn = voltages[busColumn];
            changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeColumn] =
                admittance.Magnitude * voltageRow.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
            changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow] +=
                admittance.Magnitude * voltageColumn.Magnitude * Math.Sin(voltageRow.Phase - admittance.Phase - voltageColumn.Phase);
        }
    }
}
