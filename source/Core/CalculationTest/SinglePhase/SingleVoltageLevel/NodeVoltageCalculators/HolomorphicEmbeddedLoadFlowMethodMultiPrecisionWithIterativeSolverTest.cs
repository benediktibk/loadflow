using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodMultiPrecisionWithIterativeSolverTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 0.0001; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new HolomorphicEmbeddedLoadFlowMethod(0.0000001, 70, 100, true);
        }
    }
}
