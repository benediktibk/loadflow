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

        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations)
            : base(targetPrecision, maximumIterations)
        {
            _solver = new BiCgStab();
            _preconditioner = new ILU0Preconditioner();
        }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages, double residualImprovementFactor)
        {
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(pqBuses.Count + pvBuses.Count);
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);
            var pqBusToMatrixIndex = CreateMappingBusToMatrixIndex(pqBuses);
            var busToMatrixIndex = CreateMappingBusToMatrixIndex(allNodes);
            var realPowerMaximumError = powersRealError.Select(Math.Abs).Max();
            var imaginaryPowerMaximumError = pqBuses.Count > 0 ? powersImaginaryError.Select(Math.Abs).Max() : 0;
            var powerMaximumError = Math.Max(realPowerMaximumError, imaginaryPowerMaximumError);
            var angleChange = new DenseVector(allNodes.Count);
            var iterator = new Iterator<double>(new ResidualStopCriterion<double>(powerMaximumError*residualImprovementFactor));
            Matrix<double> changeMatrixRealPowerByAngle;
            Matrix<double> changeMatrixImaginaryPowerByAmplitude;
            CalculateChangeMatrices(admittances, voltages, constantCurrents, busToMatrixIndex, pqBusToMatrixIndex, out changeMatrixRealPowerByAngle, out changeMatrixImaginaryPowerByAmplitude);

            if (realPowerMaximumError > TargetPrecision)
            {
                var powersRealErrorVector = new DenseVector(powersRealError.ToArray());
                _solver.Solve(changeMatrixRealPowerByAngle, powersRealErrorVector, angleChange, iterator, _preconditioner);
            }

            if (pqBuses.Count > 0)
            {
                var amplitudeChange = new DenseVector(pqBuses.Count);

                if (imaginaryPowerMaximumError > TargetPrecision)
                {
                    var powersImaginaryErrorVector = new DenseVector(powersImaginaryError.ToArray());
                    _solver.Solve(changeMatrixImaginaryPowerByAmplitude, powersImaginaryErrorVector, amplitudeChange, iterator, _preconditioner);
                }

                foreach (var bus in pqBuses)
                    improvedVoltages[bus] =
                        Complex.FromPolarCoordinates(
                            voltages[bus].Magnitude + amplitudeChange[pqBusToMatrixIndex[bus]],
                            voltages[bus].Phase + angleChange[bus]);
            }

            for(var i = 0; i < pvBuses.Count; ++i)
                improvedVoltages[pvBuses[i]] = Complex.FromPolarCoordinates(pvBusVoltages[i], voltages[pvBuses[i]].Phase + angleChange[pvBuses[i]]);

            return improvedVoltages;
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
                    CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(voltage, current);
            }

            foreach (var bus in pqBusToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var current = constantCurrents[busIndex];
                var voltage = voltages[busIndex];
                changeMatrixImaginaryPowerByAmplitude[matrixIndex, matrixIndex] +=
                    CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPartAdmittanceIndependent(current,
                        voltage);
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
                CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
            changeMatrixRealPowerByAngle[realPowerByAngleRow, realPowerByAngleRow] +=
                CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
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
                changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow] +=
                    CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltages[busColumn], voltageRow);
                return;
            }

            if (imaginaryPowerByAmplitudeRow == imaginaryPowerByAmplitudeColumn)
            {
                changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeColumn, imaginaryPowerByAmplitudeColumn] +=
                    CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPartAdmittanceDependent(voltageRow, admittance);
                return;
            }

            var voltageColumn = voltages[busColumn];
            changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeColumn] =
                CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow, voltageColumn);
            changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow] +=
                CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltageColumn, voltageRow);
        }
    }
}
