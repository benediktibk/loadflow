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

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses, out bool voltageCollapse)
        {
            var knownPowers = new DenseVector(pqBuses.Count);

            foreach (var bus in pqBuses)
                knownPowers[bus.ID] = bus.Power;

            var ownCurrents = (knownPowers.Divide(nominalVoltage)).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            var factorization = admittances.QR();
            var determinant = factorization.Determinant;

            if (determinant.Magnitude < _singularityDetection)
                throw new ArgumentOutOfRangeException("admittances",
                    "the resulting admittance matrix is nearly singular");

            voltageCollapse = false;
            return factorization.Solve(totalCurrents);
        }
    }
}
