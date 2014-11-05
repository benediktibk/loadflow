using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Misc;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class ThreeWindingTransformerTest
    {
        private Mock<IExternalReadOnlyNode> _nodeOne;
        private Mock<IExternalReadOnlyNode> _nodeTwo;
        private Mock<IExternalReadOnlyNode> _nodeThree;
        private Node _groundNode;
        private ThreeWindingTransformer _transformer;
        private IdGenerator _idGenerator;

        [TestInitialize]
        public void SetUp()
        {
            _idGenerator = new IdGenerator();
            _nodeOne = new Mock<IExternalReadOnlyNode>();
            _nodeTwo = new Mock<IExternalReadOnlyNode>();
            _nodeThree = new Mock<IExternalReadOnlyNode>();
            _groundNode = new Node(3, 0, "");
            _transformer = new ThreeWindingTransformer(_nodeOne.Object, _nodeTwo.Object, _nodeThree.Object, 50, 60, 70, 0.2, 0.21, 0.22, 1, 2, 3, 5, 0.3, new Angle(1), new Angle(2), new Angle(3), "", _idGenerator);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptyList_AllNodesReceiveACallToAddConnectedNodes()
        {
            _transformer.AddConnectedNodes(new HashSet<IExternalReadOnlyNode>());

            _nodeOne.Verify(x => x.AddConnectedNodes(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Once);
            _nodeTwo.Verify(x => x.AddConnectedNodes(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Once);
            _nodeThree.Verify(x => x.AddConnectedNodes(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ListWithNodeOne_OnlyNodeOneReceivesACallToAddConnectedNodesOnSameVoltageLevel()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>() {_nodeOne.Object};

            _transformer.AddConnectedNodesOnSameVoltageLevel(visitedNodes);

            _nodeOne.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(visitedNodes), Times.Once);
            _nodeTwo.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
            _nodeThree.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ListWithNodeTwo_OnlyNodeTwoReceivesACallToAddConnectedNodesOnSameVoltageLevel()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>() { _nodeTwo.Object };

            _transformer.AddConnectedNodesOnSameVoltageLevel(visitedNodes);

            _nodeTwo.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(visitedNodes), Times.Once);
            _nodeOne.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
            _nodeThree.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ListWithNodeThree_OnlyNodeThreeReceivesACallToAddConnectedNodesOnSameVoltageLevel()
        {
            var visitedNodes = new HashSet<IExternalReadOnlyNode>() { _nodeThree.Object };

            _transformer.AddConnectedNodesOnSameVoltageLevel(visitedNodes);

            _nodeThree.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(visitedNodes), Times.Once);
            _nodeTwo.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
            _nodeOne.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<ISet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void GetInternalNodes_Empty_OneElement()
        {
            var nodes = _transformer.GetInternalNodes();

            Assert.AreEqual(1, nodes.Count);
        }

        [TestMethod]
        public void FillInAdmittances_MockAdmittanceMatrix_AdmittanceMatrixGot5CallsToAddConnection()
        {
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();
            _nodeOne.Setup(x => x.NominalVoltage).Returns(4);

            _transformer.FillInAdmittances(admittanceMatrix.Object, 2, _groundNode, 4);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IExternalReadOnlyNode>(), It.IsAny<IExternalReadOnlyNode>(), It.IsAny<Complex>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_transformer.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_True()
        {
            Assert.IsTrue(_transformer.NeedsGroundNode);
        }
    }
}
