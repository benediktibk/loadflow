using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
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
            var nodes = new HashSet<IExternalReadOnlyNode>();

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedElements_ItselfContained_ItselfContained()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };

            _node.AddConnectedNodes(nodes);

            Assert.IsTrue(nodes.Contains(_node));
        }

        [TestMethod]
        public void AddConnectedElements_ItselfContained_ConnectedElementGetsNoCallToAddConnectedElements()
        {
            var nodes = new HashSet<IExternalReadOnlyNode> { _node };
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Never);
        }

        [TestMethod]
        public void AddConnectedElements_ItselfNotContained_ConnectedElementGetsCallToAddConnectedElements()
        {
            var nodes = new HashSet<IExternalReadOnlyNode>();
            var otherElement = new Mock<IPowerNetElement>();
            _node.Connect(otherElement.Object);

            _node.AddConnectedNodes(nodes);

            otherElement.Verify(x => x.AddConnectedNodes(new HashSet<IExternalReadOnlyNode> { _node }), Times.Once);
        }

        [TestMethod]
        public void MustBeSlackBus_NoConnectedPowerNetElementEnforcesSlackBus_False()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsFalse(_node.MustBeSlackBus);
        }

        [TestMethod]
        public void MustBeSlackBus_OneConnectedPowerNetElementEnforcesSlackBus_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.MustBeSlackBus);
        }

        [TestMethod]
        public void MustBeSlackBus_BothConnectedPowerNetElementEnforcesSlackBus_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.MustBeSlackBus);
        }

        [TestMethod]
        public void MustBePVBus_NoConnectedPowerNetElementEnforcesPVBus_False()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsFalse(_node.MustBePVBus);
        }

        [TestMethod]
        public void MustBePVBus_OneConnectedPowerNetElementEnforcesPVBus_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.MustBePVBus);
        }

        [TestMethod]
        public void MustBePVBus_BothConnectedPowerNetElementEnforcesPVBus_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.MustBePVBus);
        }

        [TestMethod]
        public void IsOverdetermined_NoSlackAndPVBus_False()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsFalse(_node.IsOverdetermined);
        }

        [TestMethod]
        public void IsOverdetermined_TwoSlackBusses_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.IsOverdetermined);
        }

        [TestMethod]
        public void IsOverdetermined_TwoPVBusses_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(true);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.IsOverdetermined);
        }

        [TestMethod]
        public void IsOverdetermined_OnePVAndSlackBus_True()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsTrue(_node.IsOverdetermined);
        }

        [TestMethod]
        public void IsOverdetermined_OnePVBus_False()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsFalse(_node.IsOverdetermined);
        }

        [TestMethod]
        public void IsOverdetermined_OneSlackBus_False()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            Assert.IsFalse(_node.IsOverdetermined);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_NoElementEnforcesPVBus_ThrowsException()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            _node.GetVoltageMagnitudeAndRealPowerForPVBus(2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_BothElementsEnforcesPVBus_ThrowsException()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            _node.GetVoltageMagnitudeAndRealPowerForPVBus(2);
        }

        [TestMethod]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_OneElementEnforcesPVBus_GotCallToCreatePVBus()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(false);
            elementOne.Setup(x => x.GetTotalPowerForPQBus(2)).Returns(new Complex(9, 2));
            elementTwo.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.GetVoltageMagnitudeAndRealPowerForPVBus(2))
                .Returns(new Tuple<double, double>(5, 6));
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            var result = _node.GetVoltageMagnitudeAndRealPowerForPVBus(2);

            elementTwo.Verify(x => x.GetVoltageMagnitudeAndRealPowerForPVBus(2), Times.Once);
            Assert.AreEqual(5, result.Item1, 0.00001);
            Assert.AreEqual(15, result.Item2, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_NoElementEnforcesSlackBus_ThrowsException()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(false);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            _node.GetSlackVoltage(45);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_BothElementsEnforcesSlackBus_ThrowsException()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            _node.GetSlackVoltage(45);
        }

        [TestMethod]
        public void GetSlackVoltage_OneElementEnforcesSlackBus_GotCallToGetSlackVoltage()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesSlackBus).Returns(false);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            _node.GetSlackVoltage(45);

            elementTwo.Verify(x => x.GetSlackVoltage(45), Times.Once);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_OneElementSlackBusAndOnePVBus_CorrectResult()
        {
            var elementOne = new Mock<IPowerNetElement>();
            var elementTwo = new Mock<IPowerNetElement>();
            elementOne.Setup(x => x.EnforcesPVBus).Returns(true);
            elementTwo.Setup(x => x.EnforcesSlackBus).Returns(true);
            elementOne.Setup(x => x.GetTotalPowerForPQBus(3)).Returns(new Complex(4, 1));
            elementTwo.Setup(x => x.GetTotalPowerForPQBus(3)).Returns(new Complex(2, 7));
            _node.Connect(elementOne.Object);
            _node.Connect(elementTwo.Object);

            var result = _node.GetTotalPowerForPQBus(3);

            elementOne.Verify(x => x.GetTotalPowerForPQBus(3), Times.Once);
            elementTwo.Verify(x => x.GetTotalPowerForPQBus(3), Times.Once);
            ComplexAssert.AreEqual(6, 8, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Voltage_NotYetSet_ThrowsException()
        {
            var voltage = _node.Voltage;
        }

        [TestMethod]
        public void Voltage_SetTo4And5_4And5()
        {
            _node.Voltage = new Complex(4, 5);

            ComplexAssert.AreEqual(4, 5, _node.Voltage, 0.00001);
        }

        [TestMethod]
        public void UpdateVoltage_NominalVoltageSetTo2AndVoltage4And5_VoltageIs8And10()
        {
            var voltages = new Dictionary<string, Complex>() {{_node.Name, new Complex(4, 5)}};

            _node.UpdateVoltage(voltages);

            ComplexAssert.AreEqual(8, 10, _node.Voltage, 0.00001);
        }
    }
}
