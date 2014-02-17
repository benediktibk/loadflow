using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        LoadFlowCalculator
    {
        override protected Vector CalculateNodeVoltagesInternal(Matrix admittancesToKnownVoltages, Matrix admittancesToUnknownVoltages, double nominalVoltage, Vector knownVoltages, Vector knownPowers)
        {
            var rightHandSide = knownPowers;
            rightHandSide.Divide(nominalVoltage);
            rightHandSide.Subtract(admittancesToKnownVoltages.Multiply(knownVoltages));
            var factorization = admittancesToUnknownVoltages.QR();
            var result = factorization.Solve(rightHandSide);
            return new DenseVector(result.ToArray());
        }
    }
}
