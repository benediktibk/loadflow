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
            Matrix<double> changeMatrixRealPowerByAngle;
            Matrix<double> changeMatrixImaginaryPowerByAmplitude;
            CalculateChangeMatrices(admittances, voltages, constantCurrents, busToMatrixIndex, pqBusToMatrixIndex, out changeMatrixRealPowerByAngle, out changeMatrixImaginaryPowerByAmplitude);

            var factorizationRealPower = changeMatrixRealPowerByAngle.LU();
            var angleChange = factorizationRealPower.Solve(new DenseVector(powersRealError.ToArray()));

            if (pqBuses.Count > 0)
            {
                var factorizationImaginaryPower = changeMatrixImaginaryPowerByAmplitude.LU();
                var amplitudeChange = factorizationImaginaryPower.Solve(new DenseVector(powersImaginaryError.ToArray()));

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
            changeMatrixRealPowerByAngle = new DenseMatrix(busToMatrixIndex.Count,
                busToMatrixIndex.Count);
            
            if (pqBusToMatrixIndex.Count > 0)
                changeMatrixImaginaryPowerByAmplitude =
                    new DenseMatrix(pqBusToMatrixIndex.Count,
                        pqBusToMatrixIndex.Count);
            else
                changeMatrixImaginaryPowerByAmplitude = null;

            foreach (var entry in admittances.EnumerateIndexed())
            {
                var busRow = entry.Item1;
                var busColumn = entry.Item2;
                var admittance = entry.Item3;
                int imaginaryPowerByAmplitudeRow;

                if (pqBusToMatrixIndex.TryGetValue(busRow, out imaginaryPowerByAmplitudeRow))
                {
                    var voltageRow = voltages[busRow];

                    int imaginaryPowerByAmplitudeColumn;
                    if (!pqBusToMatrixIndex.TryGetValue(busColumn, out imaginaryPowerByAmplitudeColumn))
                    {
                        var voltageColumn = voltages[busColumn];
                        changeMatrixImaginaryPowerByAmplitude[imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow
                            ] +=
                            CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance, voltageColumn,
                                voltageRow);
                    }
                    else
                    {
                        if (imaginaryPowerByAmplitudeRow == imaginaryPowerByAmplitudeColumn)
                            changeMatrixImaginaryPowerByAmplitude[
                                imaginaryPowerByAmplitudeColumn, imaginaryPowerByAmplitudeColumn] +=
                                CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPartAdmittanceDependent(
                                    voltageRow, admittance);
                        else
                        {
                            var voltageColumn = voltages[busColumn];
                            changeMatrixImaginaryPowerByAmplitude[
                                imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeColumn] =
                                CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow,
                                    voltageColumn);
                            changeMatrixImaginaryPowerByAmplitude[
                                imaginaryPowerByAmplitudeRow, imaginaryPowerByAmplitudeRow
                                ] +=
                                CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(admittance,
                                    voltageColumn,
                                    voltageRow);
                        }
                    }
                }

                var realPowerByAngleRow = busToMatrixIndex[busRow];
                var realPowerByAngleColumn = busToMatrixIndex[busColumn];

                if (realPowerByAngleRow != realPowerByAngleColumn)
                {
                    var voltageRow = voltages[busRow];
                    var voltageColumn = voltages[busColumn];
                    changeMatrixRealPowerByAngle[realPowerByAngleRow, realPowerByAngleColumn] =
                        CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
                    changeMatrixRealPowerByAngle[realPowerByAngleRow, realPowerByAngleRow] +=
                        CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
                }
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
    }
}
