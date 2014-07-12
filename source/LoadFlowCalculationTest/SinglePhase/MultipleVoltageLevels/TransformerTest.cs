using System;
using System.Collections.Generic;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class TransformerTest
    {
        private Node _upperSideNode;
        private Node _lowerSideNode;
        private Transformer _transformer;

        [TestInitialize]
        public void SetUp()
        {
            _upperSideNode = new Node("upper", 10);
            _lowerSideNode = new Node("lower", 0.25);
            _transformer = new Transformer("blub", _upperSideNode, _lowerSideNode);
        }

        [TestMethod]
        public void Constructor_blubAsName_NameIsblub()
        {
            Assert.AreEqual("blub", _transformer.Name);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_UpperSideNodeHasNominalVoltage10_10()
        {
            Assert.AreEqual(10, _transformer.UpperSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void UpperSideNominalVoltage_LowerSideNodeHasNominalVoltage025_025()
        {
            Assert.AreEqual(0.25, _transformer.LowerSideNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var upperSideNode = new Mock<IExternalReadOnlyNode>();
            var lowerSideNode = new Mock<IExternalReadOnlyNode>();
            var transformer = new Transformer("blub", upperSideNode.Object, lowerSideNode.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            transformer.AddConnectedNodes(nodes);

            upperSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
            lowerSideNode.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_transformer.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_transformer.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _transformer.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            var result = _transformer.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(0, 0, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _transformer.GetSlackVoltage(45);
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
