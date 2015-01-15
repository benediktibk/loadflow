using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace ConvergenceExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            const double targetPrecision = 1e-10;
            const int maximumIterations = 1000;
            var calculators = new Dictionary<string, INodeVoltageCalculator>
            {
                {"Current Iteration, BiCGSTAB", new CurrentIteration(targetPrecision, maximumIterations, true)},
                {"Current Iteration, LU", new CurrentIteration(targetPrecision, maximumIterations, false)},
                {"Newton-Raphson, BiCGSTAB", new NewtonRaphsonMethod(targetPrecision, maximumIterations, true)},
                {"Newton-Raphson, LU", new NewtonRaphsonMethod(targetPrecision, maximumIterations, false)},
                {"FDLF, BiCGSTAB", new FastDecoupledLoadFlowMethod(targetPrecision, maximumIterations, true)},
                {"FDLF, LU", new FastDecoupledLoadFlowMethod(targetPrecision, maximumIterations, false)},
                {"HELM, 64 Bit, BiCGSTAB", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, true)},
                {"HELM, 64 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 50, 64, false)},
                {"HELM, 100 Bit, BiCGSTAB", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, true)},
                {"HELM, 100 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 70, 100, false)},
                {"HELM, 200 Bit, BiCGSTAB", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, true)},
                {"HELM, 200 Bit, LU", new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, 100, 200, false)}
            };
        }
    }
}
