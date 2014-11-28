using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        public override Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages)
        {
            var improvedVoltages = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(pqBuses.Count + pvBuses.Count);
            var allNodes = new List<int>();
            allNodes.AddRange(pqBuses);
            allNodes.AddRange(pvBuses);
            var pqBusToMatrixIndex = CreateMappingBusToMatrixIndex(pqBuses);
            var busToMatrixIndex = CreateMappingBusToMatrixIndex(allNodes);

            var angleChange = CalculateAngleChange(admittances, voltages, constantCurrents, powersRealError, busToMatrixIndex);

            if (pqBuses.Count > 0)
            {
                var amplitudeChange = CalculateAmplitudeChange(admittances, voltages, constantCurrents, powersImaginaryError,
                    pqBusToMatrixIndex);

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

        private static Vector<double> CalculateAmplitudeChange(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> constantCurrents, IEnumerable<double> powersImaginaryError, IReadOnlyDictionary<int, int> busIds)
        {
            var changeMatrix = CalculateAmplitudeChangeMatrix(admittances, voltages, constantCurrents, busIds);
            var factorizationImaginaryPower = changeMatrix.LU();
            return factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));
        }

        private static DenseMatrix CalculateAmplitudeChangeMatrix(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> constantCurrents, IReadOnlyDictionary<int, int> busToMatrixIndex)
        {
            var changeMatrix = new DenseMatrix(busToMatrixIndex.Count, busToMatrixIndex.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                int matrixRow;
                int matrixColumn;

                if (!busToMatrixIndex.TryGetValue(busRow, out matrixRow))
                    continue;

                var admittance = entry.Item3;
                var voltageRow = voltages[busRow];

                if (!busToMatrixIndex.TryGetValue(busColumn, out matrixColumn))
                {
                    var voltageColumn = voltages[busColumn];
                    changeMatrix[matrixRow, matrixRow] +=
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltageColumn,
                            voltageRow);
                    continue;
                }

                if (matrixRow == matrixColumn)
                    changeMatrix[matrixColumn, matrixColumn] +=
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPartAdmittanceDependent(voltageRow, admittance);
                else
                {
                    var voltageColumn = voltages[busColumn];
                    changeMatrix[matrixRow, matrixColumn] =
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow, voltageColumn);
                    changeMatrix[matrixRow, matrixRow] +=
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltageColumn,
                            voltageRow);
                }
            }

            foreach (var bus in busToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var current = constantCurrents[busIndex];
                var voltage = voltages[busIndex];
                changeMatrix[matrixIndex, matrixIndex] +=
                    CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPartAdmittanceIndependent(current, voltage);
            }

            return changeMatrix;
        }

        private static Vector<double> CalculateAngleChange(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> constantCurrents, IEnumerable<double> powersRealError, IReadOnlyDictionary<int, int> busIds)
        {
            var changeMatrix = CalculateAngleChangeMatrix(admittances, voltages, constantCurrents, busIds);
            var factorizationRealPower = changeMatrix.LU();
            return factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));
        }

        private static DenseMatrix CalculateAngleChangeMatrix(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages,
            IList<Complex> constantCurrents, IReadOnlyDictionary<int, int> busToMatrixIndex)
        {
            var changeMatrix = new DenseMatrix(busToMatrixIndex.Count, busToMatrixIndex.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                var matrixRow = busToMatrixIndex[busRow];
                var matrixColumn = busToMatrixIndex[busColumn];

                if (matrixRow == matrixColumn)
                    continue;

                var admittance = entry.Item3;
                var voltageRow = voltages[busRow];
                var voltageColumn = voltages[busColumn];
                changeMatrix[matrixRow, matrixColumn] =
                    CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
                changeMatrix[matrixRow, matrixRow] += 
                    CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
            }

            foreach (var bus in busToMatrixIndex)
            {
                var matrixIndex = bus.Value;
                var busIndex = bus.Key;
                var voltage = voltages[busIndex];
                var current = constantCurrents[busIndex];
                changeMatrix[matrixIndex, matrixIndex] +=
                    CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(voltage, current);
            }

            return changeMatrix;
        }
    }
}
