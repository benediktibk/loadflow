using System;
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
    }
}
