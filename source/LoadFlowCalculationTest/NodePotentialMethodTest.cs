using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class NodePotentialMethodTest :
        LoadFlowCalculatorTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new NodePotentialMethod();
        }
    }
}
