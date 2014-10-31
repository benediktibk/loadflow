namespace Database
{
    public enum NodeVoltageCalculatorSelection
    {
        NodePotential,
        CurrentIteration,
        NewtonRaphson,
        FastDecoupledLoadFlow,
        HolomorphicEmbeddedLoadFlow,
        HolomorphicEmbeddedLoadFlowHighPrecision,
        HolomorphicEmbeddedLoadFlowWithCurrentIteration,
        HolomorphicEmbeddedLoadFlowWithNewtonRaphson
    }
}
