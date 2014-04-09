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
        [TestMethod]
        public void Constructor_heinzAnd2_NameIsheinz()
        {
            var node = new Node("heinz", 2);

            Assert.AreEqual("heinz", node.Name);
        }

        [TestMethod]
        public void Constructor_heinzAnd2_NominalVoltageIs2()
        {
            var node = new Node("heinz", 2);

            Assert.AreEqual(2, node.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void Constructor_somethingValid_NoConnectedElements()
        {
            var node = new Node("heinz", 2);

            Assert.AreEqual(0, node.ConnectedElements.Count);
        }

        [TestMethod]
        public void Connect_TwoValidObjects_ConnectedElementsIsCorrect()
        {
            var node = new Node("heinz", 2);
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();

            node.Connect(elementOne.Object);
            node.Connect(elementTwo.Object);

            var connectedElements = node.ConnectedElements;
            Assert.AreEqual(2, connectedElements.Count);
            Assert.IsTrue(connectedElements.Contains(elementOne.Object));
            Assert.IsTrue(connectedElements.Contains(elementTwo.Object));
        }
    }
}
