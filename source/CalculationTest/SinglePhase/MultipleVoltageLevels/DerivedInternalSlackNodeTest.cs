using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalSlackNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(Node sourceNode, int id)
        {
            return new DerivedInternalSlackNode(sourceNode, id, new Complex(10, 1));
        }

        [TestMethod]
        public void CreateSingleVoltageNode_3AsScaleBaseForVoltage_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new Node(0, 3, 0);
            var node = CreateNode(sourceNode, 1);

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsSlackBus);
            ComplexAssert.AreEqual(10.0/3, 1.0/3, result.Voltage, 0.00001);
        }
    }
}
