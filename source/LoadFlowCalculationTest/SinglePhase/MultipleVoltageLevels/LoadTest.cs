using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class LoadTest
    {
        private Node _node;
        private Load _load;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node("heinz", 3);
            _load = new Load("hanz", new Complex(4, 1), _node);
        }

        [TestMethod]
        public void Constructor_hanzAsName_NameIshanz()
        {
            Assert.AreEqual("hanz", _load.Name);
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
            var load = new Load("feed", new Complex(123, 3), node.Object);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            load.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
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
        public void FillInAdmittances_ValidNode_NothingChanged()
        {
            var dictionary = new Dictionary<IReadOnlyNode, int>();
            var admittances = DenseMatrix.OfArray(
                new[,]
                {
                    {new Complex(2, 4), new Complex(3, 1)}, 
                    {new Complex(-3, 9), new Complex(0.3, 0.4)}
                });

            _load.FillInAdmittances(admittances, dictionary, 1);

            ComplexAssert.AreEqual(2, 4, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(3, 1, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3, 9, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(0.3, 0.4, admittances[1, 1], 0.00001);
        }
    }
}
