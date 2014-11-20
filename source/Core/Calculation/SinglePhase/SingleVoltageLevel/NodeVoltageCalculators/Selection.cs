
namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public enum Selection
    {
        NodePotential,
        CurrentIteration,
        NewtonRaphson,
        FastDecoupledLoadFlow,
        HolomorphicEmbeddedLoadFlow,
        HolomorphicEmbeddedLoadFlowWithCurrentIteration,
        HolomorphicEmbeddedLoadFlowWithNewtonRaphson
    }
}
