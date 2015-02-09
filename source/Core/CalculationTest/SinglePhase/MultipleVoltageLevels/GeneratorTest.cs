using System.Collections.Generic;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class GeneratorTest
    {
        private ExternalNode _node;
        private Generator _generator;

        [TestInitialize]
        public void SetUp()
        {
            _node = new ExternalNode(0, 103, "");
            _generator = new Generator(_node, 5, 7);
        }

        [TestMethod]
        public void Constructor_VoltageMagnitudeSetTo5_VoltageMagnitudeIs5()
        {
            Assert.AreEqual(5, _generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_RealPowerSetTo7_RealPowerIs7()
        {
            Assert.AreEqual(7, _generator.RealPower, 0.00001);
        }

        [TestMethod]
        public void NominalVoltage_Empty_103()
        {
            Assert.AreEqual(103, _generator.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var generator = new Generator(node.Object, 67, 3);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            generator.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var generator = new Generator(node.Object, 67, 3);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            generator.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_generator.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_ValidNode_EmptyList()
        {
            var result = _generator.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FillInAdmittances_ValidNode_NoChangeInAdmittanceMatrix()
        {
            _generator.FillInAdmittances(null, 1, null, 1);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_generator.NeedsGroundNode);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidPowerBase_PvNodeWithScaledValues()
        {
            var result = _generator.CreateSingleVoltageNode(5, null, true);

            var resultAsPvNode = result as PvNode;
            Assert.IsNotNull(resultAsPvNode);
            Assert.AreEqual(5.0 / 103, resultAsPvNode.VoltageMagnitude, 0.00000001);
            Assert.AreEqual(7.0 / 5, resultAsPvNode.RealPower, 0.00000001);
        }

        [TestMethod]
        public void AddDirectConnectedNodes_Empty_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>();
            var node = new Mock<IExternalReadOnlyNode>();
            var generator = new Generator(node.Object, 67, 3);

            generator.AddDirectConnectedNodes(visitedNodes);

            node.Verify(x => x.AddDirectConnectedNodes(visitedNodes), Times.Once);
        }
    }
}
