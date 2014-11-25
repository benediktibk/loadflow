using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public abstract class JacobiMatrixBasedMethod : INodeVoltageCalculator
    {
        protected JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations)
        {
            TargetPrecision = targetPrecision;
            MaximumIterations = maximumIterations;
        }

        public int MaximumIterations { get; private set; }
        public double TargetPrecision { get; private set; }

        public abstract Vector<Complex> CalculateImprovedVoltages(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages);

        public Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            Vector<Complex> currentVoltages = MathNet.Numerics.LinearAlgebra.Complex.DenseVector.OfVector(initialVoltages);
            var iterations = 0;
            IList<double> powersRealDifference;
            IList<double> powersImaginaryDifference;
            CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);
            double maximumPowerDifference;
            var pvBusVoltages = new List<double>(pvBuses.Count);
            var pvBusIds = new List<int>(pvBuses.Count);
            var pqBusIds = new List<int>(pqBuses.Count);
            pvBusVoltages.AddRange(pvBuses.Select(bus => bus.VoltageMagnitude));
            pvBusIds.AddRange(pvBuses.Select(bus => bus.Index));
            pqBusIds.AddRange(pqBuses.Select(bus => bus.Index));

            do
            {
                ++iterations;
                currentVoltages = CalculateImprovedVoltages(admittances, currentVoltages, constantCurrents, powersRealDifference, powersImaginaryDifference, pqBusIds, pvBusIds, pvBusVoltages);
                CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);
                var powersRealDifferenceAbsolute = powersRealDifference.Select(Math.Abs);
                var powersImaginaryDifferenceAbsolute = powersImaginaryDifference.Select(Math.Abs);
                var powersDifferenceAbsolute = powersRealDifferenceAbsolute.Concat(powersImaginaryDifferenceAbsolute);
                maximumPowerDifference = powersDifferenceAbsolute.Max();
            } while (maximumPowerDifference > nominalVoltage*TargetPrecision && iterations <= MaximumIterations);
            
            return currentVoltages;
        }

        public double MaximumRelativePowerError
        {
            get { return 0.1; }
        }

        public static Vector<double> CombineParts(IList<double> upperParts, IList<double> lowerParts)
        {
            var result = new DenseVector(upperParts.Count + lowerParts.Count);

            for (var i = 0; i < upperParts.Count; ++i)
                result[i] = upperParts[i];

            for (var i = 0; i < lowerParts.Count; ++i)
                result[i + upperParts.Count] = lowerParts[i];

            return result;
        }

        public static void DivideParts(IList<double> complete, int firstPartCount, int secondPartCount, out IList<double> firstParts,
            out IList<double> secondParts, out IList<double> thirdParts)
        {
            firstParts = new List<double>(firstPartCount);
            secondParts = new List<double>(secondPartCount);
            var thirdPartCount = complete.Count - firstPartCount - secondPartCount;
            thirdParts = new List<double>(thirdPartCount);

            for (var i = 0; i < firstPartCount; ++i)
                firstParts.Add(complete[i]);

            for (var i = firstPartCount; i < firstPartCount + secondPartCount; ++i)
                secondParts.Add(complete[i]);

            for (var i = firstPartCount + secondPartCount; i < complete.Count; ++i)
                thirdParts.Add(complete[i]);
        }

        public static void CalculateChangeMatrixRealPowerByAngle(Matrix<double> result, IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    var voltageRow = voltages[i];
                    if (i != k)
                    {
                        var admittance = admittances[i, k];
                        var voltageColumn = voltages[k];
                        result[startRow + row, startColumn + column] = CalculateChangeMatrixEntryRealPowerByAngle(admittance, voltageRow, voltageColumn);
                    }
                    else
                    {
                        var currentRow = currents[i];
                        var diagonalPart = CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(voltageRow, currentRow);
                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < admittances.NodeCount; ++j)
                            if (i != j)
                            {
                                var admittance = admittances[i, j];
                                var voltageColumn = voltages[j];
                                offDiagonalPart += CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(admittance, voltageRow, voltageColumn);
                            }

                        result[startRow + row, startColumn + column] = diagonalPart + offDiagonalPart;
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryRealPowerByAngleDiagonalPart(Complex voltageRow, Complex currentRow)
        {
            return (-1) * voltageRow.Magnitude * currentRow.Magnitude * Math.Sin(currentRow.Phase - voltageRow.Phase);
        }

        public static double CalculateChangeMatrixEntryRealPowerByAngleOffDiagonalPart(Complex admittance, Complex voltageRow, Complex voltageColumn)
        {
            return admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        public static double CalculateChangeMatrixEntryRealPowerByAngle(Complex admittance, Complex voltageRow, Complex voltageColumn)
        {
            return (-1) * admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        public static void CalculateChangeMatrixImaginaryPowerByAngle(Matrix<double> result, IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    if (i == k)
                        throw new ArgumentException("a node can not be of PQ- and PV-type at the same time");

                    var admittance = admittances[i, k];
                    var voltageRow = voltages[i];
                    var voltageColumn = voltages[k];
                    result[startRow + row, startColumn + column] = CalculateChangeMatrixEntryImaginaryPowerByAngle(admittance, voltageRow, voltageColumn);
                }
            }
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByAngle(Complex admittance, Complex voltageRow, Complex voltageColumn)
        {
            return (-1) * admittance.Magnitude * voltageRow.Magnitude * voltageColumn.Magnitude * Math.Cos(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        public static void CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<double> result,
            IReadOnlyAdmittanceMatrix admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    var admittance = admittances[i, k];
                    var voltageRow = voltages[i];
                    if (i != k)
                    {
                        var voltageColumn = voltages[k];
                        result[startRow + row, startColumn + column] = CalculateChangeMatrixEntryImaginaryPowerByAmplitude(admittance, voltageRow, voltageColumn);
                    }
                    else
                    {
                        var currentRow = currents[i];
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

                        result[startRow + row, startColumn + column] = diagonalPart - offDiagonalPart;
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByAmplitudeOffDiagonalPart(Complex admittance, Complex voltageColumn, Complex voltageRow)
        {
            return admittance.Magnitude * voltageColumn.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByAmplitudeDiagonalPart(Complex currentRow, Complex voltageRow, Complex admittance)
        {
            return currentRow.Magnitude * Math.Sin(currentRow.Phase - voltageRow.Phase) - 2 * admittance.Magnitude * voltageRow.Magnitude * Math.Sin(admittance.Phase);
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByAmplitude(Complex admittance, Complex voltageRow, Complex voltageColumn)
        {
            return (-1) * admittance.Magnitude * voltageRow.Magnitude * Math.Sin(admittance.Phase + voltageColumn.Phase - voltageRow.Phase);
        }

        public static void CalculateChangeMatrixRealPowerByRealPart(Matrix<double> changeMatrix, IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    var voltageRow = voltages[i];
                    var admittance = admittances[i, j];
                    changeMatrix[row + startRow, column + startColumn] = CalculateChangeMatrixEntryRealPowerByRealPartOffDiagonalPart(voltageRow, admittance);

                    if (i == j)
                    {
                        var currentRow = currents[i];
                        changeMatrix[row + startRow, column + startColumn] += CalculateChangeMatrixEntryRealPowerByRealPartDiagonalPart(currentRow);
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryRealPowerByRealPartDiagonalPart(Complex currentRow)
        {
            return currentRow.Real;
        }

        public static double CalculateChangeMatrixEntryRealPowerByRealPartOffDiagonalPart(Complex voltageRow, Complex admittance)
        {
            return voltageRow.Real * admittance.Real + voltageRow.Imaginary * admittance.Imaginary;
        }

        public static void CalculateChangeMatrixRealPowerByImaginaryPart(Matrix<double> changeMatrix, IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    var voltageRow = voltages[i];
                    var admittance = admittances[i, j];
                    changeMatrix[row + startRow, column + startColumn] = CalculateChangeMatrixEntryRealPowerByImaginaryPartOffDiagonalPart(voltageRow, admittance);

                    if (i == j)
                    {
                        var currentRow = currents[i];
                        changeMatrix[row + startRow, column + startColumn] += CalculateChangeMatrixEntryRealPowerByImaginaryPartDiagonalPart(currentRow);
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryRealPowerByImaginaryPartDiagonalPart(Complex currentRow)
        {
            return currentRow.Imaginary;
        }

        public static double CalculateChangeMatrixEntryRealPowerByImaginaryPartOffDiagonalPart(Complex voltageRow, Complex admittance)
        {
            return voltageRow.Imaginary * admittance.Real - voltageRow.Real * admittance.Imaginary;
        }

        public static void CalculateChangeMatrixImaginaryPowerByRealPart(Matrix<double> changeMatrix, IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    var voltageRow = voltages[i];
                    var admittance = admittances[i, j];
                    changeMatrix[row + startRow, column + startColumn] = CalculateChangeMatrixEntryImaginaryPowerByRealPartOffDiagonalPart(voltageRow, admittance);

                    if (i == j)
                    {
                        var currentRow = currents[i];
                        changeMatrix[row + startRow, column + startColumn] -= CalculateChangeMatrixEntryImaginaryPowerByRealPartDiagonalPart(currentRow);
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByRealPartDiagonalPart(Complex currentRow)
        {
            return currentRow.Imaginary;
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByRealPartOffDiagonalPart(Complex voltageRow, Complex admittance)
        {
            return voltageRow.Imaginary * admittance.Real - voltageRow.Real * admittance.Imaginary;
        }

        public static void CalculateChangeMatrixImaginaryPowerByImaginaryPart(Matrix<double> changeMatrix, IReadOnlyAdmittanceMatrix admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    var voltageRow = voltages[i];
                    var admittance = admittances[i, j];
                    changeMatrix[row + startRow, column + startColumn] = CalculateChangeMatrixEntryImaginaryPowerByImaginaryPartOffDiagonalPart(voltageRow, admittance);

                    if (i == j)
                    {
                        var currentRow = currents[i];
                        changeMatrix[row + startRow, column + startColumn] += CalculateChangeMatrixEntryImaginaryPowerByImaginaryPartDiagonalPart(currentRow);
                    }
                }
            }
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByImaginaryPartDiagonalPart(Complex currentRow)
        {
            return currentRow.Real;
        }

        public static double CalculateChangeMatrixEntryImaginaryPowerByImaginaryPartOffDiagonalPart(Complex voltageRow, Complex admittance)
        {
            return (-1) * (voltageRow.Real * admittance.Real + voltageRow.Imaginary * admittance.Imaginary);
        }

        public static Dictionary<int, int> CreateMappingBusIdToIndex(IList<int> buses)
        {
            var busIdToIndex = new Dictionary<int, int>();
            var busIndex = 0;

            for (var i = 0; busIndex < buses.Count; ++i)
                if (i == buses[busIndex])
                {
                    busIdToIndex[buses[busIndex]] = busIndex;
                    ++busIndex;
                }

            return busIdToIndex;
        }

        private static void CalculatePowerDifferences(IReadOnlyAdmittanceMatrix admittances, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses,
            Vector<Complex> currentVoltages, out IList<double> powersRealDifference, out IList<double> powersImaginaryDifference)
        {
            var powersCurrent = admittances.CalculateAllPowers(currentVoltages, constantCurrents);
            powersRealDifference = new List<double>(pqBuses.Count + pvBuses.Count);
            powersImaginaryDifference = new List<double>(pqBuses.Count);

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var powerIs = powersCurrent[i].Real;
                var powerShouldBe = pqBuses[i].Power.Real;
                powersRealDifference.Add(powerShouldBe - powerIs);
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                var powerIs = powersCurrent[pqBuses.Count + i].Real;
                var powerShouldBe = pvBuses[i].RealPower;
                powersRealDifference.Add(powerShouldBe - powerIs);
            }

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var powerIs = powersCurrent[i].Imaginary;
                var powerShouldBe = pqBuses[i].Power.Imaginary;
                powersImaginaryDifference.Add(powerShouldBe - powerIs);
            }
        }
    }
}