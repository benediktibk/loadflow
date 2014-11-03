using System;
using System.Collections.Generic;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Node = Calculation.SinglePhase.MultipleVoltageLevels.Node;

namespace CalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class GeneratorTest
    {
        private Node _node;
        private Generator _generator;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node(0, 103, 0, "");
            _generator = new Generator(_node, 5, 7);
        }

        [TestMethod]
        public void Constructor_VoltageMagnitudeSetTo5_VoltageMagnitudeIs5()
        {
            Assert.AreEqual(5, _generator.VoltageMagnitude, 0.00001);
        }

        [TestMethod]
        public void Constructor_RealPowerSetTo7_RealPowerIs7()
        {
            Assert.AreEqual(7, _generator.RealPower, 0.00001);
        }

        [TestMethod]
        public void NominalVoltage_Empty_103()
        {
            Assert.AreEqual(103, _generator.NominalVoltage, 0.00001);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_True()
        {
            Assert.IsTrue(_generator.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_False()
        {
            Assert.IsFalse(_generator.EnforcesSlackBus);
        }

        [TestMethod]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_CorrectResult()
        {
            var result = _generator.GetVoltageMagnitudeAndRealPowerForPVBus(10);

            Assert.AreEqual(5.0 / 103, result.Item1, 0.0001);
            Assert.AreEqual(0.7, result.Item2, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetTotalPowerForPQBus_ValidStuff_ThrowsException()
        {
            _generator.GetTotalPowerForPQBus(3);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetSlackVoltage_VoltageSetTo4And3_ThrowsException()
        {
            _generator.GetSlackVoltage(45);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var generator = new Generator(node.Object, 67, 3);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            generator.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void AddConnectedNodesOnSameVoltageLevel_EmptySet_NodeGotCallToAddConnectedNodesOnSameVoltageLevel()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var generator = new Generator(node.Object, 67, 3);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            generator.AddConnectedNodesOnSameVoltageLevel(nodes);

            node.Verify(x => x.AddConnectedNodesOnSameVoltageLevel(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_generator.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_ValidNode_EmptyList()
        {
            var result = _generator.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FillInAdmittances_ValidNode_NoChangeInAdmittanceMatrix()
        {
            _generator.FillInAdmittances(null, 1, null, 1);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_generator.NeedsGroundNode);
        }
    }
}
