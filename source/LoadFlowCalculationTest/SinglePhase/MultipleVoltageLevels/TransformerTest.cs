using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;
using Node = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.Node;

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
            _transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 2, 1);
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
            var transformer = new Transformer("blub", upperSideNode.Object, lowerSideNode.Object, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 2, 1);
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
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 40, 1);

            Assert.IsTrue(transformer.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_RelativeRatioNot1_True()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 41, 1);

            Assert.IsTrue(transformer.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_RelativeRatio1AndNoMainImpedance_False()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 40, 1);

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
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 40, 1);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(0, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NotNominalRatioAndNoMainImpedance_ResultCountIs3()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 41, 1);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(3, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NotNominalRatioAndMainImpedance_ResultCountIs3()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 41, 1);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(3, internalNodes.Count);
        }

        [TestMethod]
        public void GetInternalNodes_NominalRatioAndMainImpedance_ResultCountIs1()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 40, 1);

            var internalNodes = transformer.GetInternalNodes();

            Assert.AreEqual(1, internalNodes.Count);
        }

        [TestMethod]
        public void FillInAdmittances_NominalRatioAndNoMainImpedance_OneConnection()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 40, 1);
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            transformer.FillInAdmittances(admittanceMatrix.Object, 1, null);

            admittanceMatrix.Verify(x => x.AddConnection(_upperSideNode, _lowerSideNode, It.IsAny<Complex>()),
                Times.Once);
            admittanceMatrix.Verify(x => x.AddGyrator(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddVoltageControlledCurrentSource(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>(), 1),
                Times.Never);
        }

        [TestMethod]
        public void FillInAdmittances_NominalRatioAndMainImpedance_ThreeConnections()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 40, 1);
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            transformer.FillInAdmittances(admittanceMatrix.Object, 1, null);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
            admittanceMatrix.Verify(x => x.AddGyrator(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddVoltageControlledCurrentSource(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>()),
                Times.Never);
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>(), It.IsAny<double>()),
                Times.Never);
        }

        [TestMethod]
        public void FillInAdmittances_NotNominalRatioAndMainImpedance_ThreeConnectionsAndOneIdealTransformer()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(5, 6), 41, 1);
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            transformer.FillInAdmittances(admittanceMatrix.Object, 1, null);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>(), It.IsAny<double>()),
                Times.Once);
        }

        [TestMethod]
        public void FillInAdmittances_NotNominalRatioAndNoMainImpedance_TwoConnectionsAndOneIdealTransformer()
        {
            var transformer = new Transformer("blub", _upperSideNode, _lowerSideNode, new Complex(1, 2), new Complex(3, 4), new Complex(), 41, 1);
            var admittanceMatrix = new Mock<IAdmittanceMatrix>();

            transformer.FillInAdmittances(admittanceMatrix.Object, 1, null);

            admittanceMatrix.Verify(x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(2));
            admittanceMatrix.Verify(x => x.AddIdealTransformer(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<double>(), It.IsAny<double>()),
                Times.Once);
        }

        [TestMethod]
        public void FillInAdmittances_NominalRatioAndNoMainImpedance_ResultIsCorrect()
        {
            var upperSideNode = new Node("upper", 1000);
            var lowerSideNode = new Node("lower", 400);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>() { { upperSideNode, 0 }, { lowerSideNode, 1 } };
            var admittances = new AdmittanceMatrix(2, nodeIndexes);
            var transformer = new Transformer("blub", upperSideNode, lowerSideNode, new Complex(2.46875, 0), new Complex(0.592499, 0), new Complex(), 2.5, 1);

            transformer.FillInAdmittances(admittances, 1, null);

            var values = admittances.GetValues();
            ComplexAssert.AreEqual(162025.45, 0, values[0, 0], 0.1);
            ComplexAssert.AreEqual(162025.45, 0, values[1, 1], 0.1);
            ComplexAssert.AreEqual(-162025.45, 0, values[0, 1], 0.1);
            ComplexAssert.AreEqual(-162025.45, 0, values[1, 0], 0.1);
        }
    }
}
