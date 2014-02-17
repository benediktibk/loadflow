using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        LoadFlowCalculator
    {
        override public Vector CalculateNodeVoltagesInternal(Matrix admittancesToKnownVoltages, Matrix admittancesToUnknownVoltages,
            double nominalVoltage, Vector knownVoltages, Vector knownPowers)
        {
            var ownCurrents = knownPowers.Divide(nominalVoltage);
            var otherCurrents = admittancesToKnownVoltages.Multiply(knownVoltages);
            var totalCurrents = ownCurrents.Subtract(otherCurrents);
            var factorization = admittancesToUnknownVoltages.QR();
            var result = factorization.Solve(totalCurrents);
            return new DenseVector(result.ToArray());
        }
    }
}
