using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class CurrentIteration : LoadFlowCalculator
    {
        private readonly int _maximumIterations;
        private readonly double _terminationCriteria;

        public CurrentIteration(double terminationCriteria, int maximumIterations)
        {
            _terminationCriteria = terminationCriteria;
            _maximumIterations = maximumIterations;
        }

        public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse,
            Vector<Complex> initialVoltages)
        {
            var knownPowers = new DenseVector(pqBuses.Count);

            foreach (var bus in pqBuses)
                knownPowers[bus.ID] = bus.Power;

            var voltages = initialVoltages;
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

            voltageCollapse = iterations > _maximumIterations;
            return voltages;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse)
        {
            var nodeCount = admittances.RowCount;
            var initialVoltages = new DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                initialVoltages[i] = new Complex(nominalVoltage, 0);

            return CalculateUnknownVoltages(admittances, nominalVoltage, constantCurrents, pqBuses, pvBuses,
                out voltageCollapse, initialVoltages);
        }
    }
}