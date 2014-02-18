using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class CurrentIteration :
        LoadFlowCalculator
    {
        private readonly double _terminationCriteria;
        private readonly int _maximumIterations;

        public CurrentIteration(double terminationCriteria, int maximumIterations)
        {
            _terminationCriteria = terminationCriteria;
            _maximumIterations = maximumIterations;
        }

        override public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittancesToKnownVoltages, Matrix<Complex> admittancesToUnknownVoltages,
            double nominalVoltage, Vector<Complex> knownVoltages, Vector<Complex> knownPowers)
        {
            throw new NotImplementedException();
        }

        override public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> knownPowers)
        {
            var nodeCount = admittances.RowCount;
            var initialVoltages = new Complex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                initialVoltages[i] = new Complex(nominalVoltage, 0);

            Vector<Complex> voltages = new DenseVector(initialVoltages);
            var iterations = 0;
            var knownPowersConjugated = knownPowers.Conjugate();
            var factorization = admittances.QR();
            double voltageChange;

            do
            {
                var currents = knownPowersConjugated.PointwiseDivide(voltages.Conjugate());
                var newVoltages = factorization.Solve(currents);
                var voltageDifference = newVoltages.Subtract(voltages);
                var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
                voltageChange = maximumVoltageDifference.Magnitude/nominalVoltage;
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && voltageChange > _terminationCriteria);

            return voltages;
        }
    }
}
