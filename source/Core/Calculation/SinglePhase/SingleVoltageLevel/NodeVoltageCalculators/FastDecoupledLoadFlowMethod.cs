using System.Collections.Generic;
using System.Diagnostics;
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

            var angleChange = CalculateAngleChange(admittances, voltages, constantCurrents, powersRealError, allNodes);

            if (pqBuses.Count > 0)
            {
                var amplitudeChange = CalculateAmplitudeChange(admittances, voltages, constantCurrents, powersImaginaryError,
                    pqBuses);

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

        private static Vector<double> CalculateAmplitudeChange(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IEnumerable<double> powersImaginaryError, IList<int> pqBuses)
        {
            var changeMatrixImaginaryPower = new DenseMatrix(pqBuses.Count, pqBuses.Count);

            for (var row = 0; row < pqBuses.Count; ++row)
            {
                var i = pqBuses[row];

                for (var column = 0; column < pqBuses.Count; ++column)
                {
                    var k = pqBuses[column];

                    var admittance = admittances[i, k];
                    var voltageRow = voltages[i];
                    if (i != k)
                    {
                        var voltageColumn = voltages[k];
                        changeMatrixImaginaryPower[0 + row, 0 + column] = CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow, voltageColumn);
                    }
                    else
                    {
                        var currentRow = constantCurrents[i];
                        var diagonalPart = CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPart(currentRow, voltageRow, admittance);

                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < admittances.NodeCount; ++j)
                        {
                            if (j != i)
                            {
                                var admittance2 = admittances[i, j];
                                var voltageColumn = voltages[j];
                                offDiagonalPart += CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance2, voltageColumn, voltageRow);
                            }
                        }

                        changeMatrixImaginaryPower[0 + row, 0 + column] = diagonalPart - offDiagonalPart;
                    }
                }
            }

            var factorizationImaginaryPower = changeMatrixImaginaryPower.LU();
            return factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));
        }

        private static Vector<double> CalculateAngleChange(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IEnumerable<double> powersRealError, IList<int> allNodes)
        {
            var changeMatrixRealPower = new DenseMatrix(allNodes.Count, allNodes.Count);

            for (var row = 0; row < allNodes.Count; ++row)
            {
                var i = allNodes[row];

                for (var column = 0; column < allNodes.Count; ++column)
                {
                    var k = allNodes[column];

                    var voltageRow = voltages[i];
                    if (i != k)
                    {
                        var admittance = admittances[i, k];
                        var voltageColumn = voltages[k];
                        changeMatrixRealPower[0 + row, 0 + column] = CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
                    }
                    else
                    {
                        var currentRow = constantCurrents[i];
                        var diagonalPart = CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(voltageRow, currentRow);
                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < admittances.NodeCount; ++j)
                            if (i != j)
                            {
                                var admittance = admittances[i, j];
                                var voltageColumn = voltages[j];
                                offDiagonalPart += CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
                            }

                        changeMatrixRealPower[0 + row, 0 + column] = diagonalPart + offDiagonalPart;
                    }
                }
            }

            var factorizationRealPower = changeMatrixRealPower.LU();
            return factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));
        }
    }
}
