using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Factorization;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class CurrentIteration :
        LoadFlowCalculator
    {
        private readonly int _maximumIterations;
        private readonly double _terminationCriteria;

        public CurrentIteration(double terminationCriteria, int maximumIterations)
        {
            _terminationCriteria = terminationCriteria;
            _maximumIterations = maximumIterations;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            int nodeCount = admittances.RowCount;
            var initialVoltages = new Complex[nodeCount];

            for (int i = 0; i < nodeCount; ++i)
                initialVoltages[i] = new Complex(nominalVoltage, 0);

            Vector<Complex> voltages = new DenseVector(initialVoltages);
            int iterations = 0;
            Vector<Complex> knownPowersConjugated = knownPowers.Conjugate();
            QR factorization = admittances.QR();
            double voltageChange;

            do
            {
                Vector<Complex> loadCurrents = knownPowersConjugated.PointwiseDivide(voltages.Conjugate());
                Vector<Complex> currents = loadCurrents.Add(constantCurrents);
                Vector<Complex> newVoltages = factorization.Solve(currents);
                Vector<Complex> voltageDifference = newVoltages.Subtract(voltages);
                Complex maximumVoltageDifference = voltageDifference.AbsoluteMaximum();
                voltageChange = maximumVoltageDifference.Magnitude/nominalVoltage;
                voltages = newVoltages;
                ++iterations;
            } while (iterations <= _maximumIterations && voltageChange > _terminationCriteria);

            if (iterations > _maximumIterations)
                throw new NotConvergingException();

            return voltages;
        }
    }
}