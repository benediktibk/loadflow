using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalPQNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(Node sourceNode, string name)
        {
            return new DerivedInternalPQNode(sourceNode, name, new Complex(4, -2));
        }

        [TestMethod]
        public void CreateSingleVoltageNode_2AsScaleBaseForPower_PowerOfPQBusIsCorrectScaled()
        {
            var sourceNode = new Node("source", 3);
            var node = CreateNode(sourceNode, "pqbus");

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsPQBus);
            ComplexAssert.AreEqual(2, -1, result.Power, 0.00001);
        }
    }
}
