using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Misc;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class TransmissionLineTest
    {
        private TransmissionLine _transmissionLineInvalid;
        private Node _sourceNodeInvalid;
        private Node _targetNodeInvalid;
        private TransmissionLine _transmissionLineWithOnlyLengthValues;
        private TransmissionLine _transmissionLineWithLengthAndShuntValues;
        private Node _sourceNodeValid;
        private Node _targetNodeValid;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNodeInvalid = new Node(0, 102, "");
            _targetNodeInvalid = new Node(1, 12, "");
            _transmissionLineInvalid = new TransmissionLine(_sourceNodeInvalid, _targetNodeInvalid, 50, 40, 30, 20, 0.1, 10, true);
            _sourceNodeValid = new Node(0, 100, "");
            _targetNodeValid = new Node(1, 100, "");
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
            var groundNode = new Node(-1, 0, "");
            var admittances = new Mock<IAdmittanceMatrix>();

            _transmissionLineWithLengthAndShuntValues.FillInAdmittances(admittances.Object, 10, groundNode, 1);

            admittances.Verify(
                x => x.AddConnection(It.IsAny<IReadOnlyNode>(), It.IsAny<IReadOnlyNode>(), It.IsAny<Complex>()),
                Times.Exactly(3));
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_transmissionLineWithOnlyLengthValues.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_transmissionLineWithOnlyLengthValues.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _transmissionLineWithOnlyLengthValues.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            var result = _transmissionLineWithOnlyLengthValues.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(0, 0, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _transmissionLineWithOnlyLengthValues.GetSlackVoltage(45);
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
    }
}
