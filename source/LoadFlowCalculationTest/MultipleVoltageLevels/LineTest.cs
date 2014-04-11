using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.MultipleVoltageLevels
{
    [TestClass]
    public class LineTest
    {
        private Line _lineInvalid;
        private Node _sourceNodeInvalid;
        private Node _targetNodeInvalid;
        private Line _lineValid;
        private Node _sourceNodeValid;
        private Node _targetNodeValid;

        [TestInitialize]
        public void SetUp()
        {
            _sourceNodeInvalid = new Node("source", 102);
            _targetNodeInvalid = new Node("target", 12);
            _lineInvalid = new Line("connect", _sourceNodeInvalid, _targetNodeInvalid, 5, 4, 10);
            _sourceNodeValid = new Node("source", 100);
            _targetNodeValid = new Node("target", 100);
            _lineValid = new Line("connect", _sourceNodeValid, _targetNodeValid, 5, 4, 10);
        }

        [TestMethod]
        public void Constructor_NameSetToconnect_NameIsconnect()
        {
            Assert.AreEqual("connect", _lineInvalid.Name);
        }

        [TestMethod]
        public void Constructor_ValidValues_LengthImpedanceIsCorrect()
        {
            ComplexAssert.AreEqual(5, 4*2*Math.PI*10, _lineInvalid.LengthImpedance, 0.00001);
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
            var source = new Mock<IReadOnlyNode>();
            var target = new Mock<IReadOnlyNode>();
            var line = new Line("blub", source.Object, target.Object, 5, 4, 3);
            var nodes = new HashSet<IReadOnlyNode>();

            line.AddConnectedNodes(nodes);

            source.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
            target.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void FillInAdmittances_OnlyLengthImpedance_CorrectValuesInMatrix()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(1, 2), new Complex(-2, 3) }, { new Complex(-3, 4), new Complex(2, 1) } });
            var nodeIndexes = new Dictionary<IReadOnlyNode, int> {{_sourceNodeValid, 0}, {_targetNodeValid, 1}};

            _lineValid.FillInAdmittances(admittances, nodeIndexes, 10);

            ComplexAssert.AreEqual(1.79125857823813, -37.772994183725, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(-2.79125857823813, 42.772994183725, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3.79125857823813, 43.772994183725, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(2.79125857823813, -38.772994183725, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_lineValid.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_lineValid.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _lineValid.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            var result = _lineValid.GetTotalPowerForPQBus(3);

            ComplexAssert.AreEqual(0, 0, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _lineValid.GetSlackVoltage(45);
        }
    }
}
