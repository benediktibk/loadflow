using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalPVNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(Node sourceNode, int id)
        {
            return new DerivedInternalPVNode(sourceNode, id, 5, 4);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidScaleBases_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new Node(0, 3, 0);
            var node = CreateNode(sourceNode, 1);

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsPVBus);
            Assert.AreEqual(5.0 / 3, result.VoltageMagnitude, 0.00001);
            Assert.AreEqual(2, result.RealPower, 0.00001);
        }
    }
}
