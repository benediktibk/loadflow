using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNodeTest
    {
        private Node _sourceNode;
        private DerivedInternalNode _node;

        protected abstract DerivedInternalNode CreateNode(Node sourceNode, string name);

        [TestInitialize]
        public void SetUp()
        {
            _sourceNode = new Node("mainNode", 234);
            _node = CreateNode(_sourceNode, "derivedNode");
        }

        [TestMethod]
        public void Constructor_NameSetToValidString_NameIsCorrect()
        {
            Assert.AreEqual("derivedNode", _node.Name);
        }

        [TestMethod]
        public void Constructor_SourceNodeWithNominalVoltage234_NominalVoltageIs234()
        {
            Assert.AreEqual(234, _node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidScaleBaseForPower_ResultIsEitherSlackPVOrPQBus()
        {
            var result = _node.CreateSingleVoltageNode(2);

            Assert.IsTrue(result.IsPQBus || result.IsPVBus || result.IsSlackBus);
        }
    }
}
