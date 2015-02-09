using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LoadTest
    {
        private ExternalNode _node;
        private Load _load;

        [TestInitialize]
        public void SetUp()
        {
            _node = new ExternalNode(0, 3, "");
            _load = new Load(new Complex(4, 1), _node);
        }

        [TestMethod]
        public void Constructor_ValidLoad_LoadIsCorrect()
        {
            ComplexAssert.AreEqual(4, 1, _load.Value, 0.00001);
        }

        [TestMethod]
        public void Constructor_ValidNode_NodeHasNoConnectedElements()
        {
            Assert.AreEqual(0, _node.ConnectedElements.Count);
        }

        [TestMethod]
        public void NominalVoltage_NodeHasNominalVoltageOf3_3()
        {
            Assert.AreEqual(3, _load.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var load = new Load(new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var load = new Load(new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_load.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_ValidNode_EmptyList()
        {
            var result = _load.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FillInAdmittances_ValidNode_NoChangeInAdmittanceMatrix()
        {
            _load.FillInAdmittances(null, 1, null, 1);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_load.NeedsGroundNode);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidPowerBase_PqNodeWithScaledPower()
        {
            var result = _load.CreateSingleVoltageNode(5, null, true);

            var resultAsPqNode = result as PqNode;
            Assert.IsNotNull(resultAsPqNode);
            ComplexAssert.AreEqual(4.0/5, 1.0/5, resultAsPqNode.Power, 0.000001);
        }

        [TestMethod]
        public void AddDirectConnectedNodes_Empty_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var load = new Load(new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddDirectConnectedNodes(nodes);

            node.Verify(x => x.AddDirectConnectedNodes(nodes), Times.Once);
        }
    }
}
