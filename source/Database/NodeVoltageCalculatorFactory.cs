using System;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Database
{
    static class NodeVoltageCalculatorFactory
    {
        public static INodeVoltageCalculator Create(NodeVoltageCalculatorSelection calculatorSelection)
        {
            switch (calculatorSelection)
            {
                case NodeVoltageCalculatorSelection.NodePotential:
                    return new NodePotentialMethod();
                case NodeVoltageCalculatorSelection.CurrentIteration:
                    return new CurrentIteration(0.0001, 100);
                case NodeVoltageCalculatorSelection.NewtonRaphson:
                    return new NewtonRaphsonMethod(0.0001, 100);
                case NodeVoltageCalculatorSelection.FastDecoupledLoadFlow:
                    return new FastDecoupledLoadFlowMethod(0.0001, 100);
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlow:
                    return new HolomorphicEmbeddedLoadFlowMethod(0.0001, 50, new PrecisionLongDouble(), true);
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowHighPrecision:
                    return new HolomorphicEmbeddedLoadFlowMethod(0.0001, 100, new PrecisionMulti(200), true);
                default:
                    throw new ArgumentOutOfRangeException("calculatorSelection");
            }
        }
    }
}
