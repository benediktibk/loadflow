using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class NodePotentialMethodTest : NodeVoltageCalculatorTest
    {
        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new NodePotentialMethod();
        }
    }
}
