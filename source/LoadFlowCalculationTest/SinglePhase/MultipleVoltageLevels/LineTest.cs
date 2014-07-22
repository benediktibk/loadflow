using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;
using Node = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.Node;
using AdmittanceMatrix = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LineTest
    {
        private Line _lineInvalid;
        private Node _sourceNodeInvalid;
        private Node _targetNodeInvalid;
        private Line _lineWithOnlyLengthValues;
        private Line _lineWithLengthAndShuntValues;
        private Node _sourceNodeValid;
        private Node _targetNodeValid;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNodeInvalid = new Node("source", 102);
            _targetNodeInvalid = new Node("target", 12);
            _lineInvalid = new Line("connect", _sourceNodeInvalid, _targetNodeInvalid, 5, 4, 3, 2, 10);
            _sourceNodeValid = new Node("source", 100);
            _targetNodeValid = new Node("target", 100);
            _lineWithOnlyLengthValues = new Line("connect", _sourceNodeValid, _targetNodeValid, 5, 4, 0, 0, 10);
            _lineWithLengthAndShuntValues = new Line("connect", _sourceNodeValid, _targetNodeValid, 5, 4, 3, 2, 10);
        }

        [TestMethod]
        public void Constructor_NameSetToconnect_NameIsconnect()
        {
            Assert.AreEqual("connect", _lineInvalid.Name);
        }

        [TestMethod]
        public void Constructor_OnlyLengthValues_LengthImpedanceIsCorrect()
        {
            ComplexAssert.AreEqual(5, 4 * 2 * Math.PI * 10, _lineWithOnlyLengthValues.LengthImpedance, 0.00001);
        }

        [TestMethod]
        public void Constructor_LengthAndShuntValues_LengthImpedanceAndShuntAdmittanceIsCorrect()
        {
            ComplexAssert.AreEqual(-10.1139581511325, -12.3601714830373, _lineWithLengthAndShuntValues.LengthImpedance, 0.00001);
            ComplexAssert.AreEqual(0.905151500442966, -0.0466578682016948, _lineWithLengthAndShuntValues.ShuntAdmittance, 0.00001);
        }

        [TestMethod]
        public void SourceNominalVoltage_SourceNodeVoltageSetTo102_102()
        {
            Assert.AreEqual(102, _lineInvalid.SourceNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void TargetNominalVoltage_TargetNodeVoltageSetTo12_12()
        {
            Assert.AreEqual(12, _lineInvalid.TargetNominalVoltage, 0.00001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_SourceAndTargetGetCallToAddConnectedNodes()
        {
            var source = new Mock<IExternalReadOnlyNode>();
            var target = new Mock<IExternalReadOnlyNode>();
            var line = new Line("blub", source.Object, target.Object, 5, 4, 3, 2, 10);
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

            _lineWithOnlyLengthValues.FillInAdmittances(admittances, 1, null, 1);

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

            _lineWithOnlyLengthValues.FillInAdmittances(admittances, 10, null, 1);

            ComplexAssert.AreEqual(1.079125857823813, -1.9772994183725, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(-2.079125857823813, 6.9772994183725, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3.079125857823813, 7.9772994183725, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(2.079125857823813, -2.9772994183725, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void FillInAdmittances_LengthAdmittanceAndShuntAdmittance_CorrectValuesInMatrix()
        {
            var groundNode = new Node("ground", 0);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int> { { _sourceNodeValid, 0 }, { _targetNodeValid, 1 }, { groundNode, 2 } };
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,]
                {
                    { new Complex(1, 2), new Complex(-2, 3), new Complex(-3, 2) }, 
                    { new Complex(-3, 4), new Complex(2, 1), new Complex(1, 4) }, 
                    {new Complex(-3, 1), new Complex(10, 43), new Complex(-3, -4) }
                }), nodeIndexes);

            _lineWithLengthAndShuntValues.FillInAdmittances(admittances, 10, groundNode, 1);

            ComplexAssert.AreEqual(866.499180634597, 3.80085243052449, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(36.6523198083696, -44.4587206322193, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(-908.151500442966, 47.6578682016948, admittances[2, 0], 0.00001);
            ComplexAssert.AreEqual(37.6523198083696, -45.4587206322193, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(867.499180634597, 2.80085243052449, admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-895.151500442966, 89.6578682016948, admittances[2, 1], 0.00001);
            ComplexAssert.AreEqual(-908.151500442966, 48.6578682016948, admittances[0, 2], 0.00001);
            ComplexAssert.AreEqual(-904.151500442966, 50.6578682016948, admittances[1, 2], 0.00001);
            ComplexAssert.AreEqual(1807.30300088593, -97.3157364033897, admittances[2, 2], 0.00001);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_lineWithOnlyLengthValues.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_lineWithOnlyLengthValues.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _lineWithOnlyLengthValues.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            var result = _lineWithOnlyLengthValues.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(0, 0, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _lineWithOnlyLengthValues.GetSlackVoltage(45);
        }

        [TestMethod]
        public void NominalVoltagesMatch_ValidLine_True()
        {
            Assert.IsTrue(_lineWithOnlyLengthValues.NominalVoltagesMatch);
        }

        [TestMethod]
        public void NominalVoltagesMatch_InvalidLine_False()
        {
            Assert.IsFalse(_lineInvalid.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_LengthAndShuntImpedance_EmptyList()
        {
            var result = _lineWithLengthAndShuntValues.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void NeedsGroundNode_NoShuntAdmittance_False()
        {
            Assert.IsFalse(_lineWithOnlyLengthValues.NeedsGroundNode);
        }

        [TestMethod]
        public void NeedsGroundNode_WithShuntAdmittance_True()
        {
            Assert.IsTrue(_lineWithLengthAndShuntValues.NeedsGroundNode);
        }
    }
}
