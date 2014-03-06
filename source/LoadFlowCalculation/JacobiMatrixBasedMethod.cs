using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

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

        public abstract Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, Vector<double> powersRealError, Vector<double> powersImaginaryError);

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            var knownPowers = new DenseVector(pqBuses.Count);

            foreach (var bus in pqBuses)
                knownPowers[bus.ID] = bus.Power;

            var nodeCount = admittances.RowCount;
            var powersReal = ExtractRealParts(knownPowers);
            var powersImaginary = ExtractImaginaryParts(knownPowers);
            double voltageChange;
            var iterations = 0;
            var currentVoltages =
                CombineRealAndImaginaryParts(CreateInitialRealVoltages(nominalVoltage, nodeCount),
                CreateInitialImaginaryVoltages(nominalVoltage, nodeCount));

            do
            {
                ++iterations;
                Vector<double> lastPowersReal;
                Vector<double> lastPowersImaginary;
                CalculateRealAndImaginaryPowers(admittances, currentVoltages, constantCurrents, out lastPowersReal, out lastPowersImaginary);
                var powersRealDifference = powersReal - lastPowersReal;
                var powersImaginaryDifference = powersImaginary - lastPowersImaginary;
                var voltageChanges = CalculateVoltageChanges(admittances, currentVoltages, constantCurrents, powersRealDifference, powersImaginaryDifference);
                voltageChange = Math.Abs(voltageChanges.AbsoluteMaximum().Magnitude);
                currentVoltages = currentVoltages + voltageChanges;
            } while (voltageChange > nominalVoltage*_targetPrecision && iterations <= _maximumIterations);
            
            return currentVoltages;
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

        public static void CalculateRealAndImaginaryPowers(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, 
            out Vector<double> powersReal, out Vector<double> powersImaginary)
        {
            var currents = admittances.Multiply(voltages) - constantCurrents;
            var powers = voltages.PointwiseMultiply(currents.Conjugate());
            powersReal = ExtractRealParts(powers);
            powersImaginary = ExtractImaginaryParts(powers);
        }

        public static void CalculateChangeMatrixRealPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int row, int column)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageOne = voltages[i];
                var voltageOneAmplitude = voltageOne.Magnitude;
                var voltageOneAngle = voltageOne.Phase;

                for (var j = 0; j < nodeCount; ++j)
                {
                    if (i != j)
                    {
                        var voltageTwo = voltages[j];
                        var voltageTwoAmplitude = voltageTwo.Magnitude;
                        var voltageTwoAngle = voltageTwo.Phase;
                        var admittance = admittances[i, j];
                        var admittanceAmplitude = admittance.Magnitude;
                        var admittanceAngle = admittance.Phase;
                        var sine = Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);

                        result[row + i, column + j] = (1) * admittanceAmplitude * voltageOneAmplitude * voltageTwoAmplitude * sine;
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;
                        var constantCurrentPart = voltageOneAmplitude * currentMagnitude *
                                                  Math.Sin(currentAngle - voltageOneAngle);

                        result[row + i, column + j] = (-1) * constantCurrentPart;
                    }
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += result[row + i, column + j];

                result[row + i, column + i] = result[row + i, column + i] + sum;
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int row, int column)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageOne = voltages[i];
                var voltageOneAmplitude = voltageOne.Magnitude;
                var voltageOneAngle = voltageOne.Phase;

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittance = admittances[i, j];
                    var admittanceAmplitude = admittance.Magnitude;
                    var admittanceAngle = admittance.Phase;

                    if (i != j)
                    {
                        var voltageTwo = voltages[j];
                        var voltageTwoAngle = voltageTwo.Phase;

                        result[row + i, column + j] = (-1) * admittanceAmplitude * voltageOneAmplitude *
                                             Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;
                        var diagonalPart = currentMagnitude * Math.Sin(currentAngle - voltageOneAngle) -
                                             2 * admittanceAmplitude * voltageOneAmplitude * Math.Sin(admittanceAngle);

                        var offDiagonalPart = 0.0;

                        for (var k = 0; k < nodeCount; ++k)
                            if (k != i)
                            {
                                var voltageThree = voltages[k];
                                var voltageThreeMagnitude = voltageThree.Magnitude;
                                var voltageThreeAngle = voltageThree.Phase;
                                var admittanceOff = admittances[i, k];
                                var admittanceOffMagnitude = admittanceOff.Magnitude;
                                var admittanceOffAngle = admittanceOff.Phase;
                                offDiagonalPart += admittanceOffMagnitude*voltageThreeMagnitude*
                                                   Math.Sin(admittanceOffAngle + voltageThreeAngle - voltageOneAngle);
                            }

                        result[row + i, column + j] = diagonalPart - offDiagonalPart;
                    }
                }
            }
        }
    }
}
