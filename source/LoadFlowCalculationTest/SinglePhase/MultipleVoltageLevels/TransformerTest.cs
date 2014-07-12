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
            _transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 2);
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
            var transformer = new Transformer("blub", upperSideNode.Object, lowerSideNode.Object, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 2);
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
        public void NeedsGroundNode_MainImpedanceSet_True()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 40);

            Assert.IsTrue(transformer.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_RelativeRatioNot1_True()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 41);

            Assert.IsTrue(transformer.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_RelativeRatio1AndNoMainImpedance_False()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 40);

            Assert.IsFalse(transformer.NeedsGroundNode);
        }

        [TestMethod]
        public void Ratio_Empty_2()
        {
            Assert.AreEqual(2, _transformer.Ratio, 0.000001);
        }

        [TestMethod]
        public void NominalRatio_Empty_40()
        {
            Assert.AreEqual(40, _transformer.NominalRatio, 0.000001);
        }

        [TestMethod]
        public void RelativeRatio_Empty_CorrectResult()
        {
            Assert.AreEqual(2.0/40, _transformer.RelativeRatio, 0.000001);
        }

        [TestMethod]
        public void UpperSideImpedance_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(1, 2, _transformer.UpperSideImpedance, 0.00001);
        }

        [TestMethod]
        public void LowerSideImpedance_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(3, 4, _transformer.LowerSideImpedance, 0.00001);
        }

        [TestMethod]
        public void MainImpedance_Empty_CorrectResult()
        {
            ComplexAssert.AreEqual(5, 6, _transformer.MainImpedance, 0.00001);
        }

        [TestMethod]
        public void GetInternalNodes_NominalRatioAndNoMainImpedance_ResultCountIs0()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 40);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(0, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NotNominalRatioAndNoMainImpedance_ResultCountIs2()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 41);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(2, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NotNominalRatioAndMainImpedance_ResultCountIs2()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 41);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(2, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NominalRatioAndMainImpedance_ResultCountIs1()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 40);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(1, internalNodes.Count);
        }
    }
}
