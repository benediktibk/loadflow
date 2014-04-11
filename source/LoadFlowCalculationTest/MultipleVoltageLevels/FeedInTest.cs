using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class FeedInTest
    {
        private Node _node;
        private FeedIn _feedIn;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("nnnode", 1000);
            _feedIn = new FeedIn("feedIt", _node, new Complex(4, 3));
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
        public void NominalVoltage_Empty_SameAsNode()
        {
            Assert.AreEqual(1000, _feedIn.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IReadOnlyNode>();
            var feedIn = new FeedIn("feed", node.Object, new Complex(123, 3));
            var nodes = new HashSet<IReadOnlyNode>();

            feedIn.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
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
            _feedIn.GetVoltageMagnitudeAndRealPowerForPVBus(3, 1);
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
            var result = _feedIn.GetSlackVoltage(3);

            ComplexAssert.AreEqual(4.0/3, 1, result, 0.00001);
        }
    }
}
