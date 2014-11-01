using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class NodePotentialMethod : INodeVoltageCalculator
    {
        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqBus> pqBuses, IList<PvBus> pvBuses)
        {
            Vector<Complex> knownPowers;
            Vector<Complex> knownVoltages;

            if (pvBuses.Count == 0)
            {
                knownPowers = new DenseVector(pqBuses.Count);

                foreach (var bus in pqBuses)
                    knownPowers[bus.Id] = bus.Power;

                return CalculateUnknownVoltagesInternal(admittances, initialVoltages, constantCurrents,
                    knownPowers);
            }
            
            if (pqBuses.Count == 0)
            {
                knownVoltages = new DenseVector(pvBuses.Count);

                for (var i = 0; i < pvBuses.Count; ++i)
                    knownVoltages[i] = new Complex(pvBuses[i].VoltageMagnitude, 0);

                return knownVoltages;
            }

            knownVoltages = new DenseVector(pvBuses.Count);
            knownPowers = new DenseVector(pqBuses.Count);
            var reducedNominalVoltages = new DenseVector(pqBuses.Count);
            var indexOfNodesWithKnownVoltage = new List<int>(pvBuses.Count);
            var indexOfNodesWithUnkownVoltage = new List<int>(pqBuses.Count);

            for (var i = 0; i < pqBuses.Count; ++i)
            {
                var bus = pqBuses[i];
                knownPowers[bus.Id] = bus.Power;
                indexOfNodesWithUnkownVoltage.Add(bus.Id);
                reducedNominalVoltages[i] = initialVoltages[bus.Id];
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                var bus = pvBuses[i];
                indexOfNodesWithKnownVoltage.Add(bus.Id);
                knownVoltages[i] = new Complex(bus.VoltageMagnitude, 0);
            }

            Vector<Complex> additionalConstantCurrents;
            var admittancesReduced = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnkownVoltage, indexOfNodesWithKnownVoltage,
                knownVoltages, out additionalConstantCurrents);
            var reducedConstantCurrents = new DenseVector(indexOfNodesWithUnkownVoltage.Count);

            for (var i = 0; i < indexOfNodesWithUnkownVoltage.Count; ++i)
                reducedConstantCurrents[i] = constantCurrents[indexOfNodesWithUnkownVoltage[i]];

            var totalConstantCurrents = reducedConstantCurrents.Add(additionalConstantCurrents);

            var unknownVoltages = CalculateUnknownVoltagesInternal(admittancesReduced, reducedNominalVoltages, totalConstantCurrents,
                knownPowers);
            return PowerNetComputable.CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnkownVoltage, unknownVoltages);
        }

        public double GetMaximumPowerError()
        {
            return 10;
        }

        private Vector<Complex> CalculateUnknownVoltagesInternal(AdmittanceMatrix admittances, IEnumerable<Complex> nominalVoltages,
            Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var ownCurrents = (knownPowers.PointwiseDivide(DenseVector.OfEnumerable(nominalVoltages))).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            var factorization = admittances.CalculateFactorization();
            return factorization.Solve(totalCurrents);
        }
    }
}
