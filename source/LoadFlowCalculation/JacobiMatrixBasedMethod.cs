using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVector = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace LoadFlowCalculation
{
    public abstract class JacobiMatrixBasedMethod : LoadFlowCalculator
    {
        private readonly double _initialRealVoltage;
        private readonly double _initialImaginaryVoltage;
        private readonly double _targetPrecision;
        private readonly int _maximumIterations;

        protected JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations, double initialRealVoltage, double initialImaginaryVoltage, double maximumPowerError) : base(maximumPowerError)
        {
            _initialRealVoltage = initialRealVoltage;
            _initialImaginaryVoltage = initialImaginaryVoltage;
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
        }

        public abstract Vector<Complex> CalculateImprovedVoltages(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<double> powersRealError, IList<double> powersImaginaryError, IList<int> pqBuses, IList<int> pvBuses, IList<double> pvBusVoltages);

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var nodeCount = admittances.RowCount;
            var iterations = 0;
            var currentVoltages =
                CombineRealAndImaginaryParts(CreateInitialRealVoltages(nominalVoltage, nodeCount),
                CreateInitialImaginaryVoltages(nominalVoltage, nodeCount));
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

        private static void CalculatePowerDifferences(Matrix<Complex> admittances, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses,
            Vector<Complex> currentVoltages, out IList<double> powersRealDifference, out IList<double> powersImaginaryDifference)
        {
            var powersCurrent = CalculateAllPowers(admittances, currentVoltages, constantCurrents);
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
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(upperParts.Count + lowerParts.Count);

            for (var i = 0; i < upperParts.Count; ++i)
                result[i] = upperParts[i];

            for (var i = 0; i < lowerParts.Count; ++i)
                result[i + upperParts.Count] = lowerParts[i];

            return result;
        }

        public static void DivideParts(IList<double> complete, out Vector<double> upperParts,
            out Vector<double> lowerParts)
        {
            var count = complete.Count;
            upperParts = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(count / 2);
            lowerParts = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(count / 2);

            for (var i = 0; i < count / 2; ++i)
                upperParts[i] = complete[i];

            for (var i = 0; i < count / 2; ++i)
                lowerParts[i] = complete[i + count / 2];
        }

        private Vector<double> CreateInitialRealVoltages(double nominalVoltage, int nodeCount)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = _initialRealVoltage * nominalVoltage;

            return result;
        }

        private Vector<double> CreateInitialImaginaryVoltages(double nominalVoltage, int nodeCount)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                result[i] = _initialImaginaryVoltage * nominalVoltage;

            return result;
        }

        public static Vector<double> ExtractRealParts(IList<Complex> constantCurrents)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(constantCurrents.Count);

            for (var i = 0; i < constantCurrents.Count; ++i)
                result[i] = constantCurrents[i].Real;

            return result;
        }

        public static Vector<double> ExtractImaginaryParts(IList<Complex> constantCurrents)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(constantCurrents.Count);

            for (var i = 0; i < constantCurrents.Count; ++i)
                result[i] = constantCurrents[i].Imaginary;

            return result;
        }

        public static void CalculateChangeMatrixRealPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var k = 0; k < nodeCount; ++k)
                {
                    if (i != k)
                        result[startRow + i, startColumn + k] = (-1) * admittances[i, k].Magnitude * voltages[i].Magnitude * voltages[k].Magnitude * Math.Sin(admittances[i, k].Phase + voltages[k].Phase - voltages[i].Phase);
                    else
                        result[startRow + i, startColumn + k] = (-1) * voltages[i].Magnitude * constantCurrents[i].Magnitude *
                                                  Math.Sin(constantCurrents[i].Phase - voltages[i].Phase);
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += result[startRow + i, startColumn + j];

                result[startRow + i, startColumn + i] = result[startRow + i, startColumn + i] - sum;
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var k = 0; k < nodeCount; ++k)
                {
                    if (i != k)
                        result[startRow + i, startColumn + k] = (-1) * admittances[i, k].Magnitude * voltages[i].Magnitude * voltages[k].Magnitude * Math.Cos(admittances[i, k].Phase + voltages[k].Phase - voltages[i].Phase);
                    else
                        result[startRow + i, startColumn + k] = (-1) * voltages[i].Magnitude * constantCurrents[i].Magnitude *
                                                  Math.Cos(constantCurrents[i].Phase - voltages[i].Phase);
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += result[startRow + i, startColumn + j];

                result[startRow + i, startColumn + i] = result[startRow + i, startColumn + i] - sum;
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<double> result,
            Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn, IList<int> rows)
        {
            for (var iBus = 0; iBus < rows.Count; ++iBus)
            {
                var i = rows[iBus];

                for (var kBus = 0; kBus < rows.Count; ++kBus)
                {
                    var k = rows[kBus];

                    if (i != k)
                        result[startRow + i, startColumn + k] = (-1)*admittances[i, k].Magnitude*voltages[i].Magnitude*
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

                        result[startRow + i, startColumn + k] = diagonalPart - offDiagonalPart;
                    }
                }
            }
        }

        public static void CalculateChangeMatrixRealPowerByAmplitude(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> currents, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var k = 0; k < nodeCount; ++k)
                {
                    if (i != k)
                        result[startRow + i, startColumn + k] = admittances[i, k].Magnitude * voltages[i].Magnitude *
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

                        result[startRow + i, startColumn + k] = diagonalPart + offDiagonalPart;
                    }
                }
            }
        }

        public static Vector<double> CalculateLoadCurrentRealParts(Matrix<Complex> admittances, IList<double> voltageRealParts, IList<double> voltageImaginaryParts)
        {
            var nodeCount = admittances.RowCount;
            var currents = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var sum = 0.0;

                for (var k = 0; k < nodeCount; ++k)
                {
                    var admittance = admittances[i, k];
                    var voltageReal = voltageRealParts[k];
                    var voltageImaginary = voltageImaginaryParts[k];
                    sum += admittance.Real * voltageReal - admittance.Imaginary * voltageImaginary;
                }

                currents[i] = sum;
            }

            return currents;
        }

        public static Vector<double> CalculateLoadCurrentImaginaryParts(Matrix<Complex> admittances, IList<double> voltageRealParts, IList<double> voltageImaginaryParts)
        {
            var nodeCount = admittances.RowCount;
            var currents = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
            {
                var sum = 0.0;

                for (var k = 0; k < nodeCount; ++k)
                {
                    var admittance = admittances[i, k];
                    var voltageReal = voltageRealParts[k];
                    var voltageImaginary = voltageImaginaryParts[k];
                    sum += admittance.Real * voltageImaginary + admittance.Imaginary * voltageReal;
                }

                currents[i] = sum;
            }

            return currents;
        }
        public static Matrix<double> CalculateChangeMatrixByAngleAndAmplitude(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, IList<int> pqBuses)
        {
            var nodeCount = admittances.RowCount;
            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(nodeCount * 2, nodeCount * 2);

            CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0);
            CalculateChangeMatrixRealPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, nodeCount);
            CalculateChangeMatrixImaginaryPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, nodeCount, 0);
            CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, nodeCount, nodeCount, pqBuses);

            return changeMatrix;
        }

        public static Matrix<double> CalculateChangeMatrixByRealAndImaginaryPart(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary)
        {
            Vector<double> currentsReal;
            Vector<double> currentsImaginary;
            MathNet.Numerics.LinearAlgebra.Double.DenseMatrix changeMatrix;
            InitializeChangeMatrixByRealAndImaginaryPart(admittances, voltagesReal, voltagesImaginary, constantCurrentsReal, constantCurrentsImaginary, out currentsReal, out currentsImaginary, out changeMatrix);

            var nodeCount = admittances.RowCount;
            CalculateChangeMatrixRealPowerByRealPart(admittances, voltagesReal, voltagesImaginary, changeMatrix, currentsReal, 0, 0);
            CalculateChangeMatrixRealPowerByImaginaryPart(admittances, voltagesReal, voltagesImaginary, changeMatrix, currentsImaginary, 0, nodeCount);
            CalculateChangeMatrixImaginaryPowerByRealPart(admittances, voltagesReal, voltagesImaginary, changeMatrix,
                currentsImaginary, nodeCount, 0);
            CalculateChangeMatrixImaginaryPowerByImaginaryPart(admittances, voltagesReal, voltagesImaginary, changeMatrix,
                currentsReal, nodeCount, nodeCount);

            return changeMatrix;
        }

        public static void InitializeChangeMatrixByRealAndImaginaryPart(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary,
            Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary, out Vector<double> currentsReal, out Vector<double> currentsImaginary,
            out DenseMatrix changeMatrix)
        {
            var nodeCount = voltagesReal.Count;
            var loadCurrentsReal = CalculateLoadCurrentRealParts(admittances, voltagesReal, voltagesImaginary);
            var loadCurrentsImaginary = CalculateLoadCurrentImaginaryParts(admittances, voltagesReal, voltagesImaginary);
            currentsReal = loadCurrentsReal - constantCurrentsReal;
            currentsImaginary = loadCurrentsImaginary - constantCurrentsImaginary;
            changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(nodeCount * 2, nodeCount * 2);
        }

        public static void CalculateChangeMatrixRealPowerByRealPart(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var j = 0; j < nodeCount; ++j)
                {
                    changeMatrix[i + startRow, j + startColumn] = voltagesReal[i] * admittances[i, j].Real + voltagesImaginary[i] * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsReal[i];
                }
            }
        }

        public static void CalculateChangeMatrixRealPowerByImaginaryPart(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var j = 0; j < nodeCount; ++j)
                {
                    changeMatrix[i + startRow, j + startColumn] = voltagesImaginary[i] * admittances[i, j].Real - voltagesReal[i] * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsImaginary[i];
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByRealPart(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var j = 0; j < nodeCount; ++j)
                {
                    changeMatrix[i + startRow, j + startColumn] = voltagesImaginary[i] * admittances[i, j].Real - voltagesReal[i] * admittances[i, j].Imaginary;

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] -= currentsImaginary[i];
                }
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByImaginaryPart(Matrix<Complex> admittances, IList<double> voltagesReal,
            IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal, int startRow, int startColumn)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                for (var j = 0; j < nodeCount; ++j)
                {
                    changeMatrix[i + startRow, j + startColumn] = (-1) *
                                                                 (voltagesReal[i] * admittances[i, j].Real +
                                                                  voltagesImaginary[i] * admittances[i, j].Imaginary);

                    if (i == j)
                        changeMatrix[i + startRow, i + startColumn] += currentsReal[i];
                }
            }
        }
    }
}
