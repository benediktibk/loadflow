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

        public override Vector<System.Numerics.Complex> CalculateUnknownVoltages(Matrix<System.Numerics.Complex> admittances,
            double nominalVoltage, Vector<System.Numerics.Complex> constantCurrents, Vector<System.Numerics.Complex> knownPowers)
        {
            var nodeCount = admittances.RowCount;
            var initialVoltages = new System.Numerics.Complex[nodeCount];

            for (var i = 0; i < nodeCount; ++i)
                initialVoltages[i] = new System.Numerics.Complex(nominalVoltage, 0);

            Vector<System.Numerics.Complex> voltages = new DenseVector(initialVoltages);
            var iterations = 0;
            var knownPowersConjugated = knownPowers.Conjugate();
            var factorization = admittances.QR();
            double voltageChange;

            do
            {
                var loadCurrents = knownPowersConjugated.PointwiseDivide(voltages.Conjugate());
                var currents = loadCurrents.Add(constantCurrents);
                var newVoltages = factorization.Solve(currents);
                var voltageDifference = newVoltages.Subtract(voltages);
                var maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
                voltageChange = maximumVoltageDifference.Magnitude / nominalVoltage;
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && voltageChange > _terminationCriteria);

            return voltages;
        }
    }
}
