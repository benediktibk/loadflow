using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;
using IAdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class TransmissionLineTest
    {
        private TransmissionLine _transmissionLineInvalid;
        private ExternalNode _sourceNodeInvalid;
        private ExternalNode _targetNodeInvalid;
        private TransmissionLine _transmissionLineWithOnlyLengthValues;
        private TransmissionLine _transmissionLineWithLengthAndShuntValues;
        private ExternalNode _sourceNodeValid;
        private ExternalNode _targetNodeValid;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNodeInvalid = new ExternalNode(0, 102, "");
            _targetNodeInvalid = new ExternalNode(1, 12, "");
            _transmissionLineInvalid = new TransmissionLine(_sourceNodeInvalid, _targetNodeInvalid, 50, 40, 30, 20, 0.1, 10, true);
            _sourceNodeValid = new ExternalNode(0, 100, "");
            _targetNodeValid = new ExternalNode(1, 100, "");
            _transmissionLineWithOnlyLengthValues = new TransmissionLine(_sourceNodeValid, _targetNodeValid, 50, 40, 0, 0, 0.1, 10, true);
            _transmissionLineWithLengthAndShuntValues = new TransmissionLine(_sourceNodeValid, _targetNodeValid, 50, 40, 30, 20, 0.1, 10, true);
        }

        [TestMethod]
        public void Constructor_OnlyLengthValues_LengthImpedanceIsCorrect()
        {
            ComplexAssert.AreEqual(5, 4 * 2 * Math.PI * 10, _transmissionLineWithOnlyLengthValues.LengthImpedance, 0.00001);
        }

        [TestMethod]
        public void Constructor_LengthAndShuntValues_LengthImpedanceAndShuntAdmittanceIsCorrect()
        {
            ComplexAssert.AreEqual(-10.1139581511325, -12.3601714830373, _transmissionLineWithLengthAndShuntValues.LengthImpedance, 0.00001);
            ComplexAssert.AreEqual(0.905151500442966/2, -0.0466578682016948/2, _transmissionLineWithLengthAndShuntValues.ShuntAdmittance, 0.00001);
        }

        [TestMethod]
        public void SourceNominalVoltage_SourceNodeVoltageSetTo102_102()
        {
            Assert.AreEqual(102, _transmissionLineInvalid.SourceNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void TargetNominalVoltage_TargetNodeVoltageSetTo12_12()
        {
            Assert.AreEqual(12, _transmissionLineInvalid.TargetNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var source = new Mock<IExternalReadOnlyNode>();
            var target = new Mock<IExternalReadOnlyNode>();
            var line = new TransmissionLine(source.Object, target.Object, 5, 4, 3, 2, 1, 10, true);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            line.AddConnectedNodes(nodes);

            source.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
            target.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_SourceAndTargetGetCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var source = new Mock<IExternalReadOnlyNode>();
            var target = new Mock<IExternalReadOnlyNode>();
            var line = new TransmissionLine(source.Object, target.Object, 5, 4, 3, 2, 1, 10, true);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            line.AddConnectedNodesOnSameVoltageLevel(nodes);

            source.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
            target.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void FillInAdmittances_OnlyLengthImpedance_OneCallToAddConnection()
        {
            var admittances = new Mock<IAdmittanceMatrix>();
            var scaler = new DimensionScaler(_sourceNodeValid.NominalVoltage, 10);
            var lengthAdmittanceScaled = scaler.ScaleAdmittance(1/_transmissionLineWithOnlyLengthValues.LengthImpedance);

            _transmissionLineWithOnlyLengthValues.FillInAdmittances(admittances.Object, 10, null, 1);

            admittances.Verify(x => x.AddConnection(_sourceNodeValid, _targetNodeValid, lengthAdmittanceScaled),
                Times.Once);
            admittances.Verify(
                x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Once);
        }

        [TestMethod]
        public void FillInAdmittances_LengthAdmittanceAndShuntAdmittance_ThreeCallsToAddConnection()
        {
            var groundNode = new ExternalNode(-1, 0, "");
            var admittances = new Mock<IAdmittanceMatrix>();

            _transmissionLineWithLengthAndShuntValues.FillInAdmittances(admittances.Object, 10, groundNode, 1);

            admittances.Verify(
                x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
        }

        [TestMethod]
        public void FillInAdmittances_ZeroLength_NoCallToAddConnection()
        {
            var groundNode = new ExternalNode(-1, 0, "");
            var admittances = new Mock<IAdmittanceMatrix>();
            var transmissionLine = new TransmissionLine(_sourceNodeValid, _targetNodeValid, 50, 40, 0, 0, 0, 10, true);

            transmissionLine.FillInAdmittances(admittances.Object, 10, groundNode, 1);

            admittances.Verify(
                x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Never);
        }

        [TestMethod]
        public void NominalVoltagesMatch_ValidLine_True()
        {
            Assert.IsTrue(_transmissionLineWithOnlyLengthValues.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NominalVoltagesMatch_InvalidLine_False()
        {
            Assert.IsFalse(_transmissionLineInvalid.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_LengthAndShuntImpedance_EmptyList()
        {
            var result = _transmissionLineWithLengthAndShuntValues.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NeedsGroundNode_NoShuntAdmittance_False()
        {
            Assert.IsFalse(_transmissionLineWithOnlyLengthValues.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_WithShuntAdmittance_True()
        {
            Assert.IsTrue(_transmissionLineWithLengthAndShuntValues.NeedsGroundNode);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_ValidPowerBase_PqNodeWithNoPower()
        {
            var result = _transmissionLineWithLengthAndShuntValues.CreateSingleVoltageNode(5, null);

            var resultAsPqNode = result as PqNode;
            Assert.IsNotNull(resultAsPqNode);
            ComplexAssert.AreEqual(0, 0, resultAsPqNode.Power, 0.000001);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_SourceNode_TargetNodeHasCallToCreateSingleVoltageNode()
        {
            var source = new Mock<IExternalReadOnlyNode>();
            var target = new Mock<IExternalReadOnlyNode>();
            var pqNode = new PqNode(new Complex(1234, 534));
            var line = new TransmissionLine(source.Object, target.Object, 1, 2, 3, 4, 0, 2, true);
            target.Setup(x => x.CreateSingleVoltageNode(123, It.IsAny<ISet<IExternalReadOnlyNode>>())).Returns(pqNode);
            var visited = new HashSet<IExternalReadOnlyNode> {source.Object};

            var result = line.CreateSingleVoltageNode(123, visited);

            var resultPqNode = result as PqNode;
            Assert.IsNotNull(resultPqNode);
            ComplexAssert.AreEqual(1234, 534, resultPqNode.Power, 1e-10);
        }

        [TestMethod]
        public void CreateSingleVoltageNode_TargetNode_SourceNodeHasCallToCreateSingleVoltageNode()
        {
            var source = new Mock<IExternalReadOnlyNode>();
            var target = new Mock<IExternalReadOnlyNode>();
            var pqNode = new PqNode(new Complex(1234, 534));
            var line = new TransmissionLine(source.Object, target.Object, 1, 2, 3, 4, 0, 2, true);
            source.Setup(x => x.CreateSingleVoltageNode(123, It.IsAny<ISet<IExternalReadOnlyNode>>())).Returns(pqNode);
            var visited = new HashSet<IExternalReadOnlyNode> { target.Object };

            var result = line.CreateSingleVoltageNode(123, visited);

            var resultPqNode = result as PqNode;
            Assert.IsNotNull(resultPqNode);
            ComplexAssert.AreEqual(1234, 534, resultPqNode.Power, 1e-10);
        }

        [TestMethod]
        public void GetDirectConnectedNodes_NonZeroLength_EmptyList()
        {
            var result = _transmissionLineWithLengthAndShuntValues.GetDirectConnectedNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetDirectConnectedNodes_ZeroLength_OneElement()
        {
            var line = new TransmissionLine(_sourceNodeValid, _targetNodeValid, 1, 2, 3, 4, 0, 2, true);

            var result = line.GetDirectConnectedNodes();

            Assert.AreEqual(1, result.Count);
        }
    }
}
