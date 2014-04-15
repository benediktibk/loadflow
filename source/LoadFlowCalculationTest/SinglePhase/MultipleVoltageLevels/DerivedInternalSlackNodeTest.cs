using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalSlackNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(Node sourceNode, string name)
        {
            return new DerivedInternalSlackNode(sourceNode, name, new Complex(10, 1));
        }

        [TestMethod]
        public void CreateSingleVoltageNode_3AsScaleBaseForVoltage_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new Node("source", 3);
            var node = CreateNode(sourceNode, "pqbus");

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsSlackBus);
            ComplexAssert.AreEqual(10.0/3, 1.0/3, result.Voltage, 0.00001);
        }
    }
}
