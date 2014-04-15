using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class FeedInTest
    {
        private Node _node;
        private FeedIn _feedIn;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("nnnode", 2);
            _feedIn = new FeedIn("feedIt", _node, new Complex(4, 3), 5);
        }

        [TestMethod]
        public void Constructor_NameSetTofeedIt_NameIsfeedIt()
        {
            Assert.AreEqual("feedIt", _feedIn.Name);
        }

        [TestMethod]
        public void Constructor_ValidVoltageSet_VoltageIsCorrect()
        {
            ComplexAssert.AreEqual(new Complex(4, 3), _feedIn.Voltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_ShortCircuitPowerSetTo5_ShortCircuitPowerIs5()
        {
            Assert.AreEqual(5, _feedIn.ShortCircuitPower, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ShortCircuitPowerSetToNegativeValue_ThrowsException()
        {
            new FeedIn("blub", _node, new Complex(4, 3), -4);
        }

        [TestMethod]
        public void Constructor_ShortCircuitPowerSetTo0_ThrowsNoException()
        {
            var feedIn = new FeedIn("blub", _node, new Complex(4, 3), 0);

            Assert.AreEqual(0, feedIn.ShortCircuitPower);
        }

        [TestMethod]
        public void NominalVoltage_Empty_SameAsNode()
        {
            Assert.AreEqual(2, _feedIn.NominalVoltage, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InputImpedance_ShortCircuitPowerSetTo0_ThrowsException()
        {
            var feedIn = new FeedIn("blub", _node, new Complex(4, 3), 0);
            var impedance = feedIn.InputImpedance;
        }

        [TestMethod]
        public void InputImpedance_ShortCircuitPowerNotZero_CorrectResult()
        {
            Assert.AreEqual(1.1*2*2/5, _feedIn.InputImpedance, 0.0001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var feedIn = new FeedIn("feed", node.Object, new Complex(123, 3), 6);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            feedIn.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_feedIn.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_True()
        {
            Assert.IsTrue(_feedIn.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _feedIn.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            _feedIn.GetTotalPowerForPQBus(3);
        }

        [TestMethod]
        public void GetSlackVoltage_VoltageSetTo4And3_CorrectResult()
        {
            var result = _feedIn.GetSlackVoltage(45);

            ComplexAssert.AreEqual(2, 1.5, result, 0.00001);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_feedIn.NominalVoltagesMatch);
        }
    }
}
