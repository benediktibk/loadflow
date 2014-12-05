using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class NodePotentialMethod : NodeVoltageCalculator
    {
        public override Vector<Complex> CalculateUnknownVoltages(IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses)
        {
            Vector<Complex> knownPowers;
            Vector<Complex> knownVoltages;
            Progress = 0;
            RelativePowerError = 1;

            if (pvBuses.Count == 0)
            {
                knownPowers = new DenseVector(pqBuses.Count);

                foreach (var bus in pqBuses)
                    knownPowers[bus.Index] = bus.Power;

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
                knownPowers[bus.Index] = bus.Power;
                indexOfNodesWithUnkownVoltage.Add(bus.Index);
                reducedNominalVoltages[i] = initialVoltages[bus.Index];
            }

            for (var i = 0; i < pvBuses.Count; ++i)
            {
                var bus = pvBuses[i];
                indexOfNodesWithKnownVoltage.Add(bus.Index);
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
            Progress = 1;
            RelativePowerError = 0;
            return PowerNetComputable.CombineKnownAndUnknownVoltages(indexOfNodesWithKnownVoltage, knownVoltages,
                indexOfNodesWithUnkownVoltage, unknownVoltages);
        }

        public override double MaximumRelativePowerError
        {
            get { return 10; }
        }

        private Vector<Complex> CalculateUnknownVoltagesInternal(IReadOnlyAdmittanceMatrix admittances, IEnumerable<Complex> nominalVoltages,
            Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            var voltages = DenseVector.OfEnumerable(nominalVoltages);
            var ownCurrents = (knownPowers.PointwiseDivide(voltages)).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            admittances.CalculateVoltages(voltages, totalCurrents, new BiCgStab(), new Iterator<Complex>());
            return voltages;
        }
    }
}
