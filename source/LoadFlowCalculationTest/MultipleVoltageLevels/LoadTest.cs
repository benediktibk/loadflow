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
    public class LoadTest
    {
        private Node _node;
        private Load _load;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("heinz", 3);
            _load = new Load("hanz", new Complex(4, 1), _node);
        }

        [TestMethod]
        public void Constructor_hanzAsName_NameIshanz()
        {
            Assert.AreEqual("hanz", _load.Name);
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
            var node = new Mock<IReadOnlyNode>();
            var load = new Load("feed", new Complex(123, 3), node.Object);
            var nodes = new HashSet<IReadOnlyNode>();

            load.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_load.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_load.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _load.GetVoltageMagnitudeAndRealPowerForPVBus(3, 1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_CorrectResult()
        {
            var result = _load.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(4, 1, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _load.GetSlackVoltage(3);
        }
    }
}
