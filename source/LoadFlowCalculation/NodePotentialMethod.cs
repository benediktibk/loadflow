using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        LoadFlowCalculator
    {
        override public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittancesToKnownVoltages, Matrix<Complex> admittancesToUnknownVoltages,
            double nominalVoltage, Vector<Complex> knownVoltages, Vector<Complex> knownPowers)
        {
            var ownCurrents = knownPowers.Divide(nominalVoltage);
            var otherCurrents = admittancesToKnownVoltages.Multiply(knownVoltages);
            var totalCurrents = ownCurrents.Conjugate().Subtract(otherCurrents);
            var factorization = admittancesToUnknownVoltages.QR();
            var determinant = factorization.Determinant;

            if (determinant.Magnitude < 0.0001)
                throw new NotFullRankException();

            return factorization.Solve(totalCurrents);
        }

        override public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> knownPowers)
        {
            var currents = knownPowers.Divide(nominalVoltage);
            var factorization = admittances.QR();
            var determinant = factorization.Determinant;

            if (determinant.Magnitude < 0.0001)
                throw new NotFullRankException();

            return factorization.Solve(currents.Conjugate());
        }
    }
}
