using System;
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
    public class FeedInTest
    {
        private Node _node;
        private FeedIn _feedIn;
        private IdGenerator _idGenerator;

        [TestInitialize]
        public void SetUp()
        {
            _idGenerator = new IdGenerator();
            _node = new Node(0, 2, 0);
            _feedIn = new FeedIn(_node, new Complex(4, 3), 5, _idGenerator, 1.1, 1);
        }

        [TestMethod]
        public void Constructor_ValidVoltageSet_VoltageIsCorrect()
        {
            ComplexAssert.AreEqual(new Complex(4, 3), _feedIn.Voltage, 0.0001);
        }

        [TestMethod]
        public void Constructor_ShortCircuitPowerSetTo5_ShortCircuitPowerIs5()
        {
            Assert.AreEqual(5, _feedIn.ShortCircuitPower, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ShortCircuitPowerSetToNegativeValue_ThrowsException()
        {
            new FeedIn(_node, new Complex(4, 3), -4, _idGenerator, 1.1, 1);
        }

        [TestMethod]
        public void Constructor_ShortCircuitPowerSetTo0_ThrowsNoException()
        {
            var feedIn = new FeedIn(_node, new Complex(4, 3), 0, _idGenerator, 1.1, 1);

            Assert.AreEqual(0, feedIn.ShortCircuitPower);
        }

        [TestMethod]
        public void NominalVoltage_Empty_SameAsNode()
        {
            Assert.AreEqual(2, _feedIn.NominalVoltage, 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InputImpedance_ShortCircuitPowerSetTo0_ThrowsException()
        {
            var feedIn = new FeedIn(_node, new Complex(4, 3), 0, _idGenerator, 1.1, 1);
            var impedance = feedIn.InputImpedance;
        }

        [TestMethod]
        public void InputImpedance_ShortCircuitPowerNotZero_CorrectResult()
        {
            ComplexAssert.AreEqual(1.60706086633306, 1.60706086633306, _feedIn.InputImpedance, 0.0001);
        }

        [TestMethod]
        public void AddConnectedNodes_EmptySet_NodeGotCallToAddConnectedNodes()
        {
            var node = new Mock<IExternalReadOnlyNode>();
            var feedIn = new FeedIn(node.Object, new Complex(123, 3), 6, _idGenerator, 1.1, 1);
            var nodes = new HashSet<IExternalReadOnlyNode>();

            feedIn.AddConnectedNodes(nodes);

            node.Verify(x => x.AddConnectedNodes(It.IsAny<HashSet<IExternalReadOnlyNode>>()), Times.Once);
        }

        [TestMethod]
        public void EnforcesPVBus_Empty_False()
        {
            Assert.IsFalse(_feedIn.EnforcesPVBus);
        }

        [TestMethod]
        public void EnforcesSlackBus_Empty_True()
        {
            Assert.IsTrue(_feedIn.EnforcesSlackBus);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void GetVoltageMagnitudeAndRealPowerForPVBus_ValidStuff_ThrowsException()
        {
            _feedIn.GetVoltageMagnitudeAndRealPowerForPVBus(1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetTotalPowerForPQBus_ValidStuff_0()
        {
            _feedIn.GetTotalPowerForPQBus(3);
        }

        [TestMethod]
        public void GetSlackVoltage_VoltageSetTo4And3_CorrectResult()
        {
            var result = _feedIn.GetSlackVoltage(45);

            ComplexAssert.AreEqual(2, 1.5, result, 0.00001);
        }

        [TestMethod]
        public void NominalVoltagesMatch_Empty_True()
        {
            Assert.IsTrue(_feedIn.NominalVoltagesMatch);
        }

        [TestMethod]
        public void GetInternalNodes_ShortCircuitPowerSetTo0_EmptyList()
        {
            var feedIn = new FeedIn(_node, new Complex(123, 4), 0, _idGenerator, 1.1, 1);

            var result = feedIn.GetInternalNodes();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetInternalNodes_ShortCircuitPowerNot0_OneSlackNode()
        {
            var result = _feedIn.GetInternalNodes();

            Assert.AreEqual(1, result.Count);
            var node = result[0];
            Assert.IsTrue((node as DerivedInternalSlackNode) != null);
        }

        [TestMethod]
        public void FillInAdmittances_ShortCircuitPowerSetTo0_NothingChanged()
        {
            var feedIn = new FeedIn(_node, new Complex(123, 4), 0, _idGenerator, 1.1, 1);
            var dictionary = new Dictionary<IReadOnlyNode, int>();
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new [,]
                {
                    {new Complex(2, 4), new Complex(3, 1)}, 
                    {new Complex(-3, 9), new Complex(0.3, 0.4)}
                }), dictionary);

            feedIn.FillInAdmittances(admittances, 1, null, 1);

            ComplexAssert.AreEqual(2, 4, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(3, 1, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-3, 9, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(0.3, 0.4, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void FillInAdmittances_ShortCircuitPowerNot0_NothingChanged()
        {
            var dictionary = new Dictionary<IReadOnlyNode, int>();
            var internalNodes = _feedIn.GetInternalNodes();
            var internalNode = internalNodes[0];
            dictionary[internalNode] = 0;
            dictionary[_node] = 1;
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(
                new[,]
                {
                    {new Complex(2, 4), new Complex(3, 1)}, 
                    {new Complex(-3, 9), new Complex(0.3, 0.4)}
                }), dictionary);

            _feedIn.FillInAdmittances(admittances, 3, null, 1);

            ComplexAssert.AreEqual(2.41483597829611, 3.58516402170389, admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(-3.41483597829611, 9.41483597829611, admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(2.58516402170389, 1.41483597829611, admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(0.714835978296108, -0.0148359782961078, admittances[1, 1], 0.00001);
        }

        [TestMethod]
        public void NeedsGroundNode_Empty_False()
        {
            Assert.IsFalse(_feedIn.NeedsGroundNode);
        }
    }
}
