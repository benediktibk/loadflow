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
            var pqBusIdToAmplitudeIndex = CreateMappingBusIdToIndex(pqBuses);
            var allBusIdsToAmplitudeIndex = CreateMappingBusIdToIndex(allNodes);

            var angleChange = CalculateAngleChange(admittances, voltages, constantCurrents, powersRealError, allBusIdsToAmplitudeIndex);

            if (pqBuses.Count > 0)
            {
                var amplitudeChange = CalculateAmplitudeChange(admittances, voltages, constantCurrents, powersImaginaryError,
                    pqBusIdToAmplitudeIndex);

                foreach (var bus in pqBuses)
                    improvedVoltages[bus] =
                        Complex.FromPolarCoordinates(
                            voltages[bus].Magnitude + amplitudeChange[pqBusIdToAmplitudeIndex[bus]],
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

        private static DenseMatrix CalculateAmplitudeChangeMatrix(IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages,
            IList<Complex> constantCurrents, IReadOnlyDictionary<int, int> busIds)
        {
            var changeMatrix = new DenseMatrix(busIds.Count, busIds.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var i = entry.Item1;
                var k = entry.Item2;
                int row;
                int column;

                if (!busIds.TryGetValue(i, out row) || !busIds.TryGetValue(k, out column))
                    continue;

                var admittance = entry.Item3;
                var voltageRow = voltages[i];

                if (i == k)
                {
                    var currentRow = constantCurrents[i];
                    changeMatrix[column, column] +=
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPart(currentRow, voltageRow,
                            admittance);
                }
                else
                {
                    var voltageColumn = voltages[k];
                    changeMatrix[row, column] =
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow, voltageColumn);
                    changeMatrix[row, row] +=
                        CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltageColumn,
                            voltageRow);
                }
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
            IList<Complex> constantCurrents, IReadOnlyDictionary<int, int> busIds)
        {
            var changeMatrix = new DenseMatrix(busIds.Count, busIds.Count);

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var i = entry.Item1;
                var k = entry.Item2;
                int row;
                int column;

                if (i == k || !busIds.TryGetValue(i, out row) || !busIds.TryGetValue(k, out column))
                    continue;

                var admittance = entry.Item3;
                var voltageRow = voltages[i];
                var voltageColumn = voltages[k];
                var currentRow = constantCurrents[i];
                changeMatrix[row, column] =
                    CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
                changeMatrix[column, column] +=
                    CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(voltageRow, currentRow);
                changeMatrix[row, row] += 
                    CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
            }

            return changeMatrix;
        }
    }
}
