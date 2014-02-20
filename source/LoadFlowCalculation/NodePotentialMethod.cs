using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        LoadFlowCalculator
    {
        public override Vector<System.Numerics.Complex> CalculateUnknownVoltages(Matrix<System.Numerics.Complex> admittances,
            double nominalVoltage, Vector<System.Numerics.Complex> constantCurrents, Vector<System.Numerics.Complex> knownPowers)
        {
            var ownCurrents = (knownPowers.Divide(nominalVoltage)).Conjugate();
            var totalCurrents = ownCurrents.Add(constantCurrents);
            var factorization = admittances.QR();
            var determinant = factorization.Determinant;

            if (determinant.Magnitude < 0.0001)
                throw new NotFullRankException();

            return factorization.Solve(totalCurrents);
        }
    }
}
