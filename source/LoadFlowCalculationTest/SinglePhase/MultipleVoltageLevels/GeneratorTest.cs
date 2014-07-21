using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;
using Node = LoadFlowCalculation.SinglePhase.MultipleVoltageLevels.Node;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class GeneratorTest
    {
        private Node _node;
        private Generator _generator;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("ultimateNode", 103);
            _generator = new Generator("dada", _node, 5, 7);
        }

        [TestMethod]
        public void Constructor_NameSetTodada_NameIsdada()
        {
            Assert.AreEqual("dada", _generator.Name);
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
            var generator = new Generator("feed", node.Object, 67, 3);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            generator.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
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
        public void FillInAdmittances_ValidNode_NothingChanged()
        {
            var dictionary = new Dictionary<IReadOnlyNode, int>();
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,]
                {
                    {new Complex(2, 4), new Complex(3, 1)}, 
                    {new Complex(-3, 9), new Complex(0.3, 0.4)}
                }), dictionary);

            _generator.FillInAdmittances(admittances, 1, null);

            var values = admittances.GetValues();
            ComplexAssert.AreEqual(2, 4, values[0, 0], 0.00001);
            ComplexAssert.AreEqual(3, 1, values[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3, 9, values[1, 0], 0.00001);
            ComplexAssert.AreEqual(0.3, 0.4, values[1, 1], 0.00001);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_generator.NeedsGroundNode);
        }
    }
}
