using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod : LoadFlowCalculator
    {
        private readonly double _singularityDetection;

        public NodePotentialMethod(double singualrityDetection)
        {
            _singularityDetection = singualrityDetection;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse)
        {
            voltageCollapse = false;
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

            Matrix<Complex> admittancesReduced;
            Vector<Complex> additionalConstantCurrents;
            ReduceAdmittancesByKnownVoltages(admittances, indexOfNodesWithUnkownVoltage, indexOfNodesWithKnownVoltage,
                knownVoltages, out admittancesReduced, out additionalConstantCurrents);
            var reducedConstantCurrents = new DenseVector(indexOfNodesWithUnkownVoltage.Count);

            for (var i = 0; i < indexOfNodesWithUnkownVoltage.Count; ++i)
                reducedConstantCurrents[i] = constantCurrents[indexOfNodesWithUnkownVoltage[i]];

            var totalConstantCurrents = reducedConstantCurrents.Add(additionalConstantCurrents);

            var unknownVoltages = CalculateUnknownVoltagesInternal(admittancesReduced, nominalVoltage, totalConstantCurrents,
                knownPowers);
            return CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnkownVoltage, unknownVoltages);
        }

        private Vector<Complex> CalculateUnknownVoltagesInternal(Matrix<Complex> admittances, double nominalVoltage,
            Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var ownCurrents = (knownPowers.Divide(nominalVoltage)).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            var factorization = admittances.QR();
            var determinant = factorization.Determinant;

            if (determinant.Magnitude < _singularityDetection)
                throw new ArgumentOutOfRangeException("admittances",
                    "the resulting admittance matrix is nearly singular");

            return factorization.Solve(totalCurrents);
        }
    }
}
