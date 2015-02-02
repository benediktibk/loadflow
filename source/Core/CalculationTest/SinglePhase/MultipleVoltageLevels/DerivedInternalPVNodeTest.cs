using System.Collections.Generic;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalPVNodeTest : DerivedInternalNodeTest
    {
        protected override DerivedInternalNode CreateNode(IExternalReadOnlyNode sourceNode, int id)
        {
            return new DerivedInternalPVNode(sourceNode, id, 5, 4, "");
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidScaleBases_VoltageOfPVBusIsCorrectScaled()
        {
            var sourceNode = new ExternalNode(0, 3, "");
            var node = CreateNode(sourceNode, 1);

            var result = node.CreateSingleVoltageNode(2, new HashSet<IExternalReadOnlyNode>(), true);

            var resultAsPvNode = result as PvNode;
            Assert.IsNotNull(resultAsPvNode);
            Assert.AreEqual(5.0 / 3, resultAsPvNode.VoltageMagnitude, 0.00001);
            Assert.AreEqual(2, resultAsPvNode.RealPower, 0.00001);
        }
    }
}
