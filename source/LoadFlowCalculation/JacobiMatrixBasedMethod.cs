using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using DenseVector = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace LoadFlowCalculation
{
    public abstract class JacobiMatrixBasedMethod : INodeVoltageCalculator
    {
        private readonly double _targetPrecision;
        private readonly int _maximumIterations;
        private readonly double _maximumPowerError;

        protected JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations, double maximumPowerError)
        {
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
            _maximumPowerError = maximumPowerError;
        }

        public abstract Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages);

        public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var iterations = 0;
            var initialVoltageCalculator = new NodePotentialMethod();
            var currentVoltages = initialVoltageCalculator.CalculateUnknownVoltages(admittances, totalAdmittanceRowSums,
                nominalVoltage, constantCurrents, pqBuses, pvBuses);
            IList<double> powersRealDifference;
            IList<double> powersImaginaryDifference;
            CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);
            double maximumPowerDifference;
            var pvBusVoltages = new List<double>(pvBuses.Count);
            var pvBusIds = new List<int>(pvBuses.Count);
            var pqBusIds = new List<int>(pqBuses.Count);

            foreach (var bus in pvBuses)
            {
                pvBusVoltages.Add(bus.VoltageMagnitude);
                pvBusIds.Add(bus.ID);
            }

            foreach (var bus in pqBuses)
                pqBusIds.Add(bus.ID);

            do
            {
                ++iterations;
                var improvedVoltages = CalculateImprovedVoltages(admittances, currentVoltages, constantCurrents, powersRealDifference, powersImaginaryDifference, pqBusIds, pvBusIds, pvBusVoltages);
                currentVoltages = improvedVoltages;
                CalculatePowerDifferences(admittances, constantCurrents, pqBuses, pvBuses, currentVoltages, out powersRealDifference, out powersImaginaryDifference);

                maximumPowerDifference = 0;

                foreach (var powerDifference in powersRealDifference)
                {
                    if (Math.Abs(powerDifference) > maximumPowerDifference)
                        maximumPowerDifference = Math.Abs(powerDifference);
                }

                foreach (var powerDifference in powersImaginaryDifference)
                {
                    if (Math.Abs(powerDifference) > maximumPowerDifference)
                        maximumPowerDifference = Math.Abs(powerDifference);
                }
            } while (maximumPowerDifference > nominalVoltage*_targetPrecision && iterations <= _maximumIterations);
            
            return currentVoltages;
        }

        public double GetMaximumPowerError()
        {
            return _maximumPowerError;
        }

        private static void CalculatePowerDifferences(Matrix<Complex> admittances, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses,
            Vector<Complex> currentVoltages, out IList<double> powersRealDifference, out IList<double> powersImaginaryDifference)
        {
            var powersCurrent = LoadFlowCalculator.CalculateAllPowers(admittances, currentVoltages, constantCurrents);
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

        public static Vector<Complex> CombineRealAndImaginaryParts(IList<double> realParts,
            IList<double> imaginaryParts)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(realParts.Count);

            for (var i = 0; i < result.Count; ++i)
                result[i] = new Complex(realParts[i], imaginaryParts[i]);

            return result;
        }

        public static Vector<Complex> CombineAmplitudesAndAngles(IList<double> amplitudes,
            IList<double> angles)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(amplitudes.Count);

            for (var i = 0; i < result.Count; ++i)
                result[i] = Complex.FromPolarCoordinates(amplitudes[i], angles[i]);

            return result;
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

            Debug.Assert(firstPartCount == firstParts.Count);
            Debug.Assert(secondPartCount == secondParts.Count);
            Debug.Assert(thirdPartCount == thirdParts.Count);
        }

        public static Vector<double> ExtractRealParts(IList<Complex> constantCurrents)
        {
            var result = new DenseVector(constantCurrents.Count);

            for (var i = 0; i < constantCurrents.Count; ++i)
                result[i] = constantCurrents[i].Real;

            return result;
        }

        public static Vector<double> ExtractImaginaryParts(IList<Complex> constantCurrents)
        {
            var result = new DenseVector(constantCurrents.Count);

            for (var i = 0; i < constantCurrents.Count; ++i)
                result[i] = constantCurrents[i].Imaginary;

            return result;
        }

        public static void CalculateChangeMatrixRealPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            var nodeCount = admittances.RowCount;

            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    if (i != k)
                        result[startRow + row, startColumn + column] = (-1) * admittances[i, k].Magnitude * voltages[i].Magnitude *
                                                                voltages[k].Magnitude*
                                                                Math.Sin(admittances[i, k].Phase + voltages[k].Phase -
                                                                         voltages[i].Phase);
                    else
                    {
                        var diagonalPart = (-1)*voltages[i].Magnitude*currents[i].Magnitude*
                                                                Math.Sin(currents[i].Phase - voltages[i].Phase);
                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < nodeCount; ++j)
                            if (i != j)
                                offDiagonalPart += admittances[i, j].Magnitude * voltages[i].Magnitude *
                                                                voltages[j].Magnitude *
                                                                Math.Sin(admittances[i, j].Phase + voltages[j].Phase -
                                                                         voltages[i].Phase);

                        result[startRow + row, startColumn + column] = diagonalPart + offDiagonalPart;
                    }
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            var nodeCount = admittances.RowCount;

            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    if (i != k)
                        result[startRow + row, startColumn + column] = (-1)*admittances[i, k].Magnitude*voltages[i].Magnitude*
                                                                voltages[k].Magnitude*
                                                                Math.Cos(admittances[i, k].Phase + voltages[k].Phase -
                                                                         voltages[i].Phase);
                    else
                    {
                        var diagonalPart = (-1)*voltages[i].Magnitude*currents[i].Magnitude*
                                                                Math.Cos(currents[i].Phase - voltages[i].Phase);
                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < nodeCount; ++j)
                            if (j != i)
                                offDiagonalPart += admittances[i, j].Magnitude * voltages[i].Magnitude *
                                                                    voltages[j].Magnitude *
                                                                    Math.Cos(admittances[i, j].Phase + voltages[j].Phase -
                                                                             voltages[i].Phase);

                        result[startRow + row, startColumn + column] = diagonalPart + offDiagonalPart;
                    }
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<double> result,
            Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    if (i != k)
                        result[startRow + row, startColumn + column] = (-1)*admittances[i, k].Magnitude*voltages[i].Magnitude*
                                                      Math.Sin(admittances[i, k].Phase + voltages[k].Phase - voltages[i].Phase);
                    else
                    {
                        var diagonalPart = currents[i].Magnitude*Math.Sin(currents[i].Phase - voltages[i].Phase) -
                                           2*admittances[i, k].Magnitude*voltages[i].Magnitude*Math.Sin(admittances[i, k].Phase);

                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < admittances.ColumnCount; ++j)
                        {
                            if (j != i)
                                offDiagonalPart += admittances[i, j].Magnitude*voltages[j].Magnitude*
                                                   Math.Sin(admittances[i, j].Phase + voltages[j].Phase -
                                                            voltages[i].Phase);
                        }

                        result[startRow + row, startColumn + column] = diagonalPart - offDiagonalPart;
                    }
                }
            }
        }

        public static void CalculateChangeMatrixRealPowerByAmplitude(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            var nodeCount = admittances.RowCount;

            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var k = columns[column];

                    if (i != k)
                        result[startRow + row, startColumn + column] = admittances[i, k].Magnitude * voltages[i].Magnitude *
                                             Math.Cos(admittances[i, k].Phase + voltages[k].Phase - voltages[i].Phase);
                    else
                    {
                        var diagonalPart = (-1) * currents[i].Magnitude * Math.Cos(currents[i].Phase - voltages[i].Phase) +
                                             2 * admittances[i, i].Magnitude * voltages[i].Magnitude * Math.Cos(admittances[i, i].Phase);

                        var offDiagonalPart = 0.0;

                        for (var j = 0; j < nodeCount; ++j)
                            if (j != i)
                                offDiagonalPart += admittances[i, j].Magnitude*voltages[j].Magnitude*
                                                   Math.Cos(admittances[i, j].Phase + voltages[j].Phase - voltages[i].Phase);

                        result[startRow + row, startColumn + column] = diagonalPart + offDiagonalPart;
                    }
                }
            }
        }

        public static Vector<double> CalculateLoadCurrentImaginaryParts(Matrix<Complex> admittances,
            IList<Complex> voltages)
        {
            var nodeCount = admittances.RowCount;
            var currents = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var sum = 0.0;

                for (var k = 0; k < nodeCount; ++k)
                    sum += admittances[i, k].Real*voltages[k].Imaginary + admittances[i, k].Imaginary*voltages[k].Real;

                currents[i] = sum;
            }

            return currents;
        }

        public static void CalculateChangeMatrixRealPowerByRealPart(Matrix<double> changeMatrix, Matrix<Complex> admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    changeMatrix[row + startRow, column + startColumn] = voltages[i].Real * admittances[i, j].Real + voltages[i].Imaginary * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[row + startRow, column + startColumn] += currents[i].Real;
                }
            }
        }

        public static void CalculateChangeMatrixRealPowerByImaginaryPart(Matrix<double> changeMatrix, Matrix<Complex> admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    changeMatrix[row + startRow, column + startColumn] = voltages[i].Imaginary * admittances[i, j].Real - voltages[i].Real * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[row + startRow, column + startColumn] += currents[i].Imaginary;
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByRealPart(Matrix<double> changeMatrix, Matrix<Complex> admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    changeMatrix[row + startRow, column + startColumn] = voltages[i].Imaginary * admittances[i, j].Real - voltages[i].Real * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[row + startRow, column + startColumn] -= currents[i].Imaginary;
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByImaginaryPart(Matrix<double> changeMatrix, Matrix<Complex> admittances, IList<Complex> voltages, IList<Complex> currents, int startRow, int startColumn, IList<int> rows, IList<int> columns)
        {
            for (var row = 0; row < rows.Count; ++row)
            {
                var i = rows[row];

                for (var column = 0; column < columns.Count; ++column)
                {
                    var j = columns[column];

                    changeMatrix[row + startRow, column + startColumn] = (-1) *
                                                                 (voltages[i].Real * admittances[i, j].Real +
                                                                  voltages[i].Imaginary * admittances[i, j].Imaginary);

                    if (i == j)
                        changeMatrix[row + startRow, column + startColumn] += currents[i].Real;
                }
            }
        }

        public static Dictionary<int, int> CreateMappingBusIdToIndex(IList<int> buses, int totalCount)
        {
            var busIdToAmplitudeIndex = new Dictionary<int, int>();
            var busIndex = 0;

            for (var i = 0; i < totalCount && busIndex < buses.Count; ++i)
                if (i == buses[busIndex])
                {
                    busIdToAmplitudeIndex[buses[busIndex]] = busIndex;
                    ++busIndex;
                }

            return busIdToAmplitudeIndex;
        }
    }
}
