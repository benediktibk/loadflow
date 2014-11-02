using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Misc;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LoadTest
    {
        private Node _node;
        private Load _load;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node(0, 3, 0, "");
            _load = new Load(new Complex(4, 1), _node);
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
            var node = new Mock<IExternalReadOnlyNode>();
            var load = new Load(new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var load = new Load(new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
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
            _load.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ValidStuff_CorrectResult()
        {
            var result = _load.GetTotalPowerForPQBus(1);

            ComplexAssert.AreEqual(4, 1, result, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _load.GetSlackVoltage(45);
        }

        [TestMethod]
        public void GetTotalPowerForPQBus_ScaleBasePowerSetTo2_PowerIsScaled()
        {
            var result = _load.GetTotalPowerForPQBus(2);

            ComplexAssert.AreEqual(2, 0.5, result, 0.0001);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_load.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_ValidNode_EmptyList()
        {
            var result = _load.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FillInAdmittances_ValidNode_NoChangeInAdmittanceMatrix()
        {
            _load.FillInAdmittances(null, 1, null, 1);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_load.NeedsGroundNode);
        }
    }
}
