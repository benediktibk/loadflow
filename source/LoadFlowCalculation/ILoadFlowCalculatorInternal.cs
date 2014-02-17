using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public interface ILoadFlowCalculatorInternal
    {
        Vector CalculateNodeVoltagesInternal(Matrix admittancesToKnownVoltages,
            Matrix admittancesToUnknownVoltages, double nominalVoltage, Vector knownVoltages, Vector knownPowers);
    }
}
