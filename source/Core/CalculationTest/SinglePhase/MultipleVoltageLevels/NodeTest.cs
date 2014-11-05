using System.Collections.Generic;
using System.Linq;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class NodeTest
    {
        private Node _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node(0, 2, "");
        }

        [TestMethod]
        public void Constructor_0And2_IdIs0()
        {
            Assert.AreEqual(0, _node.Id);
        }

        [TestMethod]
        public void Constructor_0And2_NominalVoltageIs2()
        {
            Assert.AreEqual(2, _node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_SomethingValid_NoConnectedElements()
        {
            Assert.AreEqual(0, _node.ConnectedElements.Count);
        }

        [TestMethod]
        public void Connect_TwoValidObjects_ConnectedElementsIsCorrect()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();

            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            var connectedElements = _node.ConnectedElements;
            Assert.AreEqual(2, connectedElements.Count);
            Assert.IsTrue(connectedElements.Contains(elementOne.Object));
            Assert.IsTrue(connectedElements.Contains(elementTwo.Object));
        }

        [TestMethod]
        public void Equals_SameObject_True()
        {
            Assert.AreEqual(_node, _node);
        }

        [TestMethod]
        public void Equals_DifferentObjectWithSameValues_False()
        {
            Assert.AreNotEqual(_node, new Node(0, 2, ""));
        }

        [TestMethod]
        public void AddConnectedNodes_ItselfNotContained_ItselfContained()
        {
            var nodes = new HashSet<IExternalReadOnlyNode>();

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedNodes_ItselfContained_ItselfContained()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedNodes_ItselfContained_ConnectedElementGetsNoCallToAddConnectedNodes()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedNodes_ItselfNotContained_ConnectedElementGetsCallToAddConnectedNodes()
        {
            var nodes = new HashSet<IExternalReadOnlyNode>();
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(new HashSet<IExternalReadOnlyNode> { _node }), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ItselfNotContained_ItselfContained()
        {
            var nodes = new HashSet<IExternalReadOnlyNode>();

            _node.AddConnectedNodesOnSameVoltageLevel(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ItselfContained_ItselfContained()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };

            _node.AddConnectedNodesOnSameVoltageLevel(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ItselfContained_ConnectedElementGetsNoCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodesOnSameVoltageLevel(nodes);

            otherElement.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_ItselfNotContained_ConnectedElementGetsCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var nodes = new HashSet<IExternalReadOnlyNode>();
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodesOnSameVoltageLevel(nodes);

            otherElement.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(new HashSet<IExternalReadOnlyNode> { _node }), Times.Once);
        }
    }
}
