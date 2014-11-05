using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PqNodeTest
    {
        private PqNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new PqNode(new Complex(3, 4));
        }

        [TestMethod]
        public void Constructor_3And4_PowerIs3And4()
        {
            ComplexAssert.AreEqual(3, 4, _node.Power, 0.000001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetVoltageIn_NullAndValidId_ThrowsException()
        {
            _node.SetVoltageIn(null, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetVoltageMagnitudeIn_NullAndValidId_ThrowsException()
        {
            _node.SetVoltageMagnitudeIn(null, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetRealPowerIn_NullAndValidId_ThrowsException()
        {
            _node.SetRealPowerIn(null, 1);
        }

        [TestMethod]
        public void SetPowerIn_ValidId_CorrectValueSet()
        {
            var powers = new DenseVector(3);

            _node.SetPowerIn(powers, 2);

            ComplexAssert.AreEqual(0, 0, powers[0], 0.0001);
            ComplexAssert.AreEqual(0, 0, powers[1], 0.0001);
            ComplexAssert.AreEqual(3, 4, powers[2], 0.0001);
        }

        [TestMethod]
        public void Merge_SlackNode_NewSlackNodeWithCorrectValues()
        {
            var slackNode = new SlackNode(new Complex(3, 4));

            var result = _node.Merge(slackNode);

            var resultAsSlackNode = result as SlackNode;
            Assert.IsNotNull(resultAsSlackNode);
            Assert.AreNotEqual(resultAsSlackNode, slackNode);
            ComplexAssert.AreEqual(resultAsSlackNode.Voltage, slackNode.Voltage, 0.000001);
        }

        [TestMethod]
        public void Merge_PvNode_NewPvNodeWithCorrectValues()
        {
            var pvNode = new PvNode(5, 7);
            var result = _node.Merge(pvNode);

            var resultAsPvNode = result as PvNode;
            Assert.IsNotNull(resultAsPvNode);
            Assert.AreNotEqual(resultAsPvNode, pvNode);
            Assert.AreEqual(8, resultAsPvNode.RealPower, 0.000001);
            Assert.AreEqual(7, resultAsPvNode.VoltageMagnitude, 0.000001);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Merge_null_ThrowsException()
        {
            _node.Merge(null);
        }

        [TestMethod]
        public void Merge_PqNode_NewPqNodeWithCorrectValues()
        {
            var pqNode = new PqNode(new Complex(6, 7));
            var result = _node.Merge(pqNode);

            var resultAsPqNode = result as PqNode;
            Assert.IsNotNull(resultAsPqNode);
            Assert.AreNotEqual(resultAsPqNode, pqNode);
            Assert.AreNotEqual(resultAsPqNode, _node);
            ComplexAssert.AreEqual(_node.Power + pqNode.Power, resultAsPqNode.Power, 0.000001);
        }
    }
}
