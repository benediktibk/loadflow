using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class NodePotentialMethod : INodeVoltageCalculator
    {
        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            Vector<Complex> knownPowers;
            Vector<Complex> knownVoltages;

            if (pvBuses.Count == 0)
            {
                knownPowers = new DenseVector(pqBuses.Count);

                foreach (var bus in pqBuses)
                    knownPowers[bus.ID] = bus.Power;

                return CalculateUnknownVoltagesInternal(admittances, nominalVoltage, constantCurrents,
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
            var indexOfNodesWithKnownVoltage = new List<int>(pvBuses.Count);
            var indexOfNodesWithUnkownVoltage = new List<int>(pqBuses.Count);
                
            foreach (var bus in pqBuses)
            {
                knownPowers[bus.ID] = bus.Power;
                indexOfNodesWithUnkownVoltage.Add(bus.ID);
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                indexOfNodesWithKnownVoltage.Add(pvBuses[i].ID);
                knownVoltages[i] = new Complex(pvBuses[i].VoltageMagnitude, 0);
            }

            Vector<Complex> additionalConstantCurrents;
            var admittancesReduced = admittances.CreateReducedAdmittanceMatrix(indexOfNodesWithUnkownVoltage, indexOfNodesWithKnownVoltage,
                knownVoltages, out additionalConstantCurrents);
            var reducedConstantCurrents = new DenseVector(indexOfNodesWithUnkownVoltage.Count);

            for (var i = 0; i < indexOfNodesWithUnkownVoltage.Count; ++i)
                reducedConstantCurrents[i] = constantCurrents[indexOfNodesWithUnkownVoltage[i]];

            var totalConstantCurrents = reducedConstantCurrents.Add(additionalConstantCurrents);

            var unknownVoltages = CalculateUnknownVoltagesInternal(admittancesReduced, nominalVoltage, totalConstantCurrents,
                knownPowers);
            return LoadFlowCalculator.CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnkownVoltage, unknownVoltages);
        }

        public double GetMaximumPowerError()
        {
            return 10;
        }

        private Vector<Complex> CalculateUnknownVoltagesInternal(AdmittanceMatrix admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var ownCurrents = (knownPowers.Divide(nominalVoltage)).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            var factorization = admittances.CalculateFactorization();
            return factorization.Solve(totalCurrents);
        }
    }
}
