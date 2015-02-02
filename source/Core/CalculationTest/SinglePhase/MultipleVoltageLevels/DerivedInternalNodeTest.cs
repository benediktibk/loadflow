using System.Collections.Generic;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNodeTest
    {
        private ExternalNode _sourceNode;
        private DerivedInternalNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNode = new ExternalNode(0, 234, "");
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
        public void CreateSingleVoltageNode_ValidScaleBaseForPower_ResultIsNotNull()
        {
            var result = _node.CreateSingleVoltageNode(2, new HashSet<IExternalReadOnlyNode>());

            Assert.IsNotNull(result);
        }

        protected abstract DerivedInternalNode CreateNode(IExternalReadOnlyNode sourceNode, int id);
    }
}
