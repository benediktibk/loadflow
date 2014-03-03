using System;
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
        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers, out bool voltageCollapse)
        {
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
