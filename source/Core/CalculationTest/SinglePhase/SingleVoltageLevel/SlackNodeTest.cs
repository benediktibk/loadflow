using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class SlackNodeTest
    {
        private SlackNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new SlackNode(new Complex(3, 4));
        }

        [TestMethod]
        public void Constructor_3And4_PowerIs3And4()
        {
            ComplexAssert.AreEqual(3, 4, _node.Voltage, 0.000001);
        }

        [TestMethod]
        public void SetVoltageIn_NullAndValidId_CorrectVoltageSet()
        {
            var voltages = new DenseVector(3);

            _node.SetVoltageIn(voltages, 0);

            ComplexAssert.AreEqual(3, 4, voltages[0], 0.0001);
            ComplexAssert.AreEqual(0, 0, voltages[1], 0.0001);
            ComplexAssert.AreEqual(0, 0, voltages[2], 0.0001);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetPowerIn_NullAndValidId_ThrowsException()
        {
            _node.SetPowerIn(null, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Merge_SlackNodeWithDifferentVoltage_ThrowsException()
        {
            _node.Merge(new SlackNode(new Complex(3, 4.1)));
        }

        [TestMethod]
        public void Merge_SlackNodeWithSameVoltage_SlackNode()
        {
            var result = _node.Merge(new SlackNode(new Complex(3, 4)));

            var resultSlackNode = result as SlackNode;
            Assert.IsNotNull(resultSlackNode);
            ComplexAssert.AreEqual(3, 4, resultSlackNode.Voltage, 1e-10);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Merge_PvNode_ThrowsException()
        {
            _node.Merge(new PvNode(3, 4));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Merge_null_ThrowsException()
        {
            _node.Merge(null);
        }

        [TestMethod]
        public void Merge_PqNode_NewSlackNodeWithCorrectValues()
        {
            var result = _node.Merge(new PqNode(new Complex(6, 7)));

            var resultAsSlackNode = result as SlackNode;
            Assert.IsNotNull(resultAsSlackNode);
            Assert.AreNotEqual(resultAsSlackNode, _node);
            ComplexAssert.AreEqual(resultAsSlackNode.Voltage, _node.Voltage, 0.000001);
        }
    }
}
