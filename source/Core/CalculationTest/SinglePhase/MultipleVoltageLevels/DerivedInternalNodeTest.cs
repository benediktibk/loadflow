using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNodeTest
    {
        private Node _sourceNode;
        private DerivedInternalNode _node;

        protected abstract DerivedInternalNode CreateNode(Node sourceNode, int id);

        [TestInitialize]
        public void SetUp()
        {
            _sourceNode = new Node(0, 234, 0, "");
            _node = CreateNode(_sourceNode, 1);
        }

        [TestMethod]
        public void Constructor_IdSetTo1_IdIs1()
        {
            Assert.AreEqual(1, _node.Id);
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
