using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalSlackNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(IExternalReadOnlyNode sourceNode, int id)
        {
            return new DerivedInternalSlackNode(sourceNode, id, new Complex(10, 1), "");
        }

        [TestMethod]
        public void CreateSingleVoltageNode_3AsScaleBaseForVoltage_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new Node(0, 3, "");
            var node = CreateNode(sourceNode, 1);

            var result = node.CreateSingleVoltageNodes(2).ToList();

            Assert.AreEqual(1, result.Count);
            var resultAsSlackNode = result.First() as SlackNode;
            Assert.IsNotNull(resultAsSlackNode);
            ComplexAssert.AreEqual(10.0 / 3, 1.0 / 3, resultAsSlackNode.Voltage, 0.00001);
        }
    }
}
