using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class NodePotentialMethodTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 0.001; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.1; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 0.07; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new NodePotentialMethod();
        }
    }
}
