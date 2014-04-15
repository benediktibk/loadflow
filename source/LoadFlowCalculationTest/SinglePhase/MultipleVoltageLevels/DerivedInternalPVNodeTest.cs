using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalPVNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(Node sourceNode, string name)
        {
            return new DerivedInternalPVNode(sourceNode, name, 5, 4);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidScaleBases_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new Node("source", 3);
            var node = CreateNode(sourceNode, "pqbus");

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsPVBus);
            Assert.AreEqual(5.0 / 3, result.VoltageMagnitude, 0.00001);
            Assert.AreEqual(2, result.RealPower, 0.00001);
        }
    }
}
