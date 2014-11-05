using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class DerivedInternalPQNodeTest : DerivedInternalNodeTest
    {
        [TestMethod]
        public void CreateSingleVoltageNode_2AsScaleBaseForPower_PowerOfPQBusIsCorrectScaled()
        {
            var sourceNode = new Node(0, 3, "");
            var node = CreateNode(sourceNode, 1);

            var result = node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsPqBus);
            ComplexAssert.AreEqual(2, -1, result.Power, 0.00001);
        }

        protected override DerivedInternalNode CreateNode(IExternalReadOnlyNode sourceNode, int id)
        {
            return new DerivedInternalPQNode(sourceNode, id, new Complex(4, -2), "");
        }
    }
}
