using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public abstract class JacobiMatrixBasedMethod : LoadFlowCalculator
    {
        private readonly double _initialRealVoltage;
        private readonly double _initialImaginaryVoltage;
        private readonly double _targetPrecision;
        private readonly int _maximumIterations;

        public JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations)
        {
            _initialRealVoltage = 1;
            _initialImaginaryVoltage = 0;
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
        }

        protected abstract Vector<double> CalculateVoltageChanges(Matrix<Complex> admittances,
            Vector<double> voltagesReal, Vector<double> voltagesImaginary,
            Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary, Vector<double> powersReal,
            Vector<double> lastPowersReal,
            Vector<double> powersImaginary, Vector<double> lastPowersImaginary);

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers,
            out bool voltageCollapse)
        {
            var nodeCount = admittances.RowCount;
            var constantCurrentsReal = ExtractRealParts(constantCurrents);
            var constantCurrentsImaginary = ExtractImaginaryParts(constantCurrents);
            var powersReal = ExtractRealParts(knownPowers);
            var powersImaginary = ExtractImaginaryParts(knownPowers);
            var voltagesReal = CreateInitialRealVoltages(nominalVoltage, nodeCount);
            var voltagesImaginary = CreateInitialImaginaryVoltages(nominalVoltage, nodeCount);
            double voltageChange;
            var iterations = 0;
            Vector<double> lastPowersReal = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);
            Vector<double> lastPowersImaginary = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(nodeCount);

            do
            {
                ++iterations;
                var voltageChanges = CalculateVoltageChanges(admittances, voltagesReal, voltagesImaginary, constantCurrentsReal, constantCurrentsImaginary, powersReal, lastPowersReal, powersImaginary, lastPowersImaginary);
                voltageChange = Math.Abs(voltageChanges.AbsoluteMaximum());
                Vector<double> voltageChangesReal;
                Vector<double> voltageChangesImaginary;
                DivideParts(voltageChanges, out voltageChangesReal, out voltageChangesImaginary);
                voltagesReal += voltageChangesReal;
                voltagesImaginary += voltageChangesImaginary;
                var currentVoltages = CombineRealAndImaginaryParts(voltagesReal, voltagesImaginary);
                var currents = admittances.Multiply(currentVoltages) - constantCurrents;
                var currentPowers = currentVoltages.PointwiseMultiply(currents.Conjugate());
                lastPowersReal = ExtractRealParts(currentPowers);
                lastPowersImaginary = ExtractImaginaryParts(currentPowers);
            } while (voltageChange > nominalVoltage*_targetPrecision && iterations <= _maximumIterations);

            voltageCollapse = voltageChange > nominalVoltage*_targetPrecision;
            return CombineRealAndImaginaryParts(voltagesReal, voltagesImaginary);
        }

        public static Vector<Complex> CombineRealAndImaginaryParts(IList<double> realParts,
            IList<double> imaginaryParts)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(realParts.Count);

            for (var i = 0; i < result.Count; ++i)
                result[i] = new Complex(realParts[i], imaginaryParts[i]);

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

        public static void CalculateLeftUpperChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i, j] = voltageReal * admittanceReal + voltageImaginary * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i, i] += currentsReal[i];
                }
            }
        }

        public static void CalculateRightUpperChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i, j + nodeCount] = voltageImaginary * admittanceReal - voltageReal * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i, i + nodeCount] += currentsImaginary[i];
                }
            }
        }

        public static void CalculateLeftLowerChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsImaginary)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + nodeCount, j] = voltageImaginary * admittanceReal - voltageReal * admittanceImaginary;

                    if (i == j)
                        changeMatrix[i + nodeCount, i] -= currentsImaginary[i];
                }
            }
        }

        public static void CalculateRightLowerChangeMatrix(Matrix<Complex> admittances, IList<double> voltagesReal, IList<double> voltagesImaginary, Matrix<double> changeMatrix, IList<double> currentsReal)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageReal = voltagesReal[i];
                var voltageImaginary = voltagesImaginary[i];

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittanceReal = admittances[i, j].Real;
                    var admittanceImaginary = admittances[i, j].Imaginary;

                    changeMatrix[i + nodeCount, j + nodeCount] = (-1) *
                                                                 (voltageReal * admittanceReal +
                                                                  voltageImaginary * admittanceImaginary);

                    if (i == j)
                        changeMatrix[i + nodeCount, i + nodeCount] += currentsReal[i];
                }
            }
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

        public static Vector<double> CalculateLoadCurrentRealParts(Matrix<Complex> admittances,
            IList<double> voltageRealParts, IList<double> voltageImaginaryParts)
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

        public static Vector<double> CalculateLoadCurrentImaginaryParts(Matrix<Complex> admittances,
            IList<double> voltageRealParts, IList<double> voltageImaginaryParts)
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
    }
}
