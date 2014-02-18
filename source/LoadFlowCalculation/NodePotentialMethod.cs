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
            var totalCurrents = ownCurrents.Subtract(otherCurrents);
            var factorization = admittancesToUnknownVoltages.QR();
            return factorization.Solve(totalCurrents);
        }

        override public Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> knownPowers)
        {
            throw new NotImplementedException();
        }
    }
}
