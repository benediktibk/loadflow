﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;

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
            _sourceNodeInvalid = new Node(0, 102, 0, "");
            _targetNodeInvalid = new Node(1, 12, 0, "");
            _transmissionLineInvalid = new TransmissionLine(_sourceNodeInvalid, _targetNodeInvalid, 50, 40, 30, 20, 0.1, 10, true);
            _sourceNodeValid = new Node(0, 100, 0, "");
            _targetNodeValid = new Node(1, 100, 0, "");
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
        public void FillInAdmittances_OnlyLengthImpedanceAndNoScaling_CorrectValuesInMatrix()
        {
            var nodeIndexes = new Dictionary<IReadOnlyNode, int> { { _sourceNodeValid, 0 }, { _targetNodeValid, 1 } };
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,] { { new Complex(1, 2), new Complex(-2, 3) }, { new Complex(-3, 4), new Complex(2, 1) } }), nodeIndexes);

            _transmissionLineWithOnlyLengthValues.FillInAdmittances(admittances, 1, null, 1);

            ComplexAssert.AreEqual(1.79125857823813, -37.772994183725, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(-2.79125857823813, 42.772994183725, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3.79125857823813, 43.772994183725, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(2.79125857823813, -38.772994183725, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void FillInAdmittances_OnlyLengthImpedanceAndScaling_CorrectValuesInMatrix()
        {
            var nodeIndexes = new Dictionary<IReadOnlyNode, int> { { _sourceNodeValid, 0 }, { _targetNodeValid, 1 } };
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,] { { new Complex(1, 2), new Complex(-2, 3) }, { new Complex(-3, 4), new Complex(2, 1) } }), nodeIndexes);

            _transmissionLineWithOnlyLengthValues.FillInAdmittances(admittances, 10, null, 1);

            ComplexAssert.AreEqual(1.079125857823813, -1.9772994183725, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(-2.079125857823813, 6.9772994183725, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3.079125857823813, 7.9772994183725, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(2.079125857823813, -2.9772994183725, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void FillInAdmittances_LengthAdmittanceAndShuntAdmittance_CorrectValuesInMatrix()
        {
            var groundNode = new Node(-1, 0, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int> { { _sourceNodeValid, 0 }, { _targetNodeValid, 1 }, { groundNode, 2 } };
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,]
                {
                    { new Complex(1, 2), new Complex(-2, 3), new Complex(-3, 2) }, 
                    { new Complex(-3, 4), new Complex(2, 1), new Complex(1, 4) }, 
                    {new Complex(-3, 1), new Complex(10, 43), new Complex(-3, -4) }
                }), nodeIndexes);

            _transmissionLineWithLengthAndShuntValues.FillInAdmittances(admittances, 10, groundNode, 1);

            ComplexAssert.AreEqual(413.923430413114, 27.1297865313719, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(36.6523198083696, -44.4587206322193, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(-455.575750221483, 24.3289341008474, admittances[2, 0], 0.00001);
            ComplexAssert.AreEqual(37.6523198083696, -45.4587206322193, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(414.923430413114, 26.1297865313719, admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-442.575750221483, 66.3289341008474, admittances[2, 1], 0.00001);
            ComplexAssert.AreEqual(-455.575750221483, 25.3289341008474, admittances[0, 2], 0.00001);
            ComplexAssert.AreEqual(-451.575750221483, 27.3289341008474, admittances[1, 2], 0.00001);
            ComplexAssert.AreEqual(902.151500442966, -50.6578682016948, admittances[2, 2], 0.00001);
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