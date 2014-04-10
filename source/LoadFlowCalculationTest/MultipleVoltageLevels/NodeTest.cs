using System;
using System.Collections.Generic;
using System.Linq;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class NodeTest
    {
        private Node _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("heinz", 2);
        }

        [TestMethod]
        public void Constructor_heinzAnd2_NameIsheinz()
        {
            Assert.AreEqual("heinz", _node.Name);
        }

        [TestMethod]
        public void Constructor_heinzAnd2_NominalVoltageIs2()
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
            Assert.AreNotEqual(_node, new Node("heinz", 2));
        }

        [TestMethod]
        public void AddConnectedElements_ItselfNotContained_ItselfContained()
        {
            var nodes = new HashSet<INode>();

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedElements_ItselfContained_ItselfContained()
        {
            var nodes = new HashSet<INode> { _node };

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedElements_ItselfContained_ConnectedElementGetsNoCallToAddConnectedElements()
        {
            var nodes = new HashSet<INode> { _node };
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<INode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedElements_ItselfNotContained_ConnectedElementGetsCallToAddConnectedElements()
        {
            var nodes = new HashSet<INode>();
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(new HashSet<INode> { _node }), Times.Once);
        }
    }
}
