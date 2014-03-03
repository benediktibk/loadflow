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

        protected JacobiMatrixBasedMethod(double targetPrecision, int maximumIterations, double initialRealVoltage, double initialImaginaryVoltage)
        {
            _initialRealVoltage = initialRealVoltage;
            _initialImaginaryVoltage = initialImaginaryVoltage;
            _targetPrecision = targetPrecision;
            _maximumIterations = maximumIterations;
        }

        public abstract Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, Vector<double> powersRealError, Vector<double> powersImaginaryError);

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse)
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

            voltageCollapse = voltageChange > nominalVoltage*_targetPrecision;
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
    }
}
