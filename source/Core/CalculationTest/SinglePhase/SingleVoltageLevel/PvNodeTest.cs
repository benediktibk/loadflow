using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PvNodeTest
    {
        private PvNode _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new PvNode(3, 4);
        }

        [TestMethod]
        public void Constructor_3And4_RealPowerIs3()
        {
            Assert.AreEqual(3, _node.RealPower, 0.000001);
        }

        [TestMethod]
        public void Constructor_3And4_VoltageMagnitudeIs4()
        {
            Assert.AreEqual(4, _node.VoltageMagnitude, 0.000001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetVoltageIn_NullAndValidId_ThrowsException()
        {
            _node.SetVoltageIn(null, 1);
        }

        [TestMethod]
        public void SetVoltageMagnitudeIn_ValidId_CorrectMagnitudeSet()
        {
            var voltages = new DenseVector(new []{new Complex(), Complex.FromPolarCoordinates(5, 6), new Complex()});

            _node.SetVoltageMagnitudeIn(voltages, 1);

            ComplexAssert.AreEqual(new Complex(), voltages[0], 0.00001);
            ComplexAssert.AreEqual(Complex.FromPolarCoordinates(4, 6), voltages[1], 0.00001);
            ComplexAssert.AreEqual(new Complex(), voltages[2], 0.00001);
        }

        [TestMethod]
        public void SetRealPowerIn_ValidId_CorrectRealPowerSet()
        {
            var powers = new DenseVector(new[] { new Complex(1, 2), new Complex(3, 4), new Complex(5, 6) });

            _node.SetRealPowerIn(powers, 0);

            ComplexAssert.AreEqual(3, 2, powers[0], 0.00001);
            ComplexAssert.AreEqual(3, 4, powers[1], 0.00001);
            ComplexAssert.AreEqual(5, 6, powers[2], 0.00001);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SetPowerIn_NullAndValidId_ThrowsException()
        {
            _node.SetPowerIn(null, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Merge_SlackNode_ThrowsException()
        {
            _node.Merge(new SlackNode(new Complex(3, 4)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Merge_PvNodeWithDifferentVoltageMagnitude_ThrowsException()
        {
            _node.Merge(new PvNode(3, 4.1));
        }

        [TestMethod]
        public void Merge_PvNodeWithSameVoltageMagnitude_PvNode()
        {
            var result = _node.Merge(new PvNode(2, 4));

            var pvNode = result as PvNode;
            Assert.IsNotNull(pvNode);
            Assert.AreEqual(4, pvNode.VoltageMagnitude, 1e-10);
            Assert.AreEqual(5, pvNode.RealPower, 1e-10);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Merge_null_ThrowsException()
        {
            _node.Merge(null);
        }

        [TestMethod]
        public void Merge_PqNode_NewPvNodeWithCorrectValues()
        {
            var result = _node.Merge(new PqNode(new Complex(6, 7)));

            var resultAsPvNode = result as PvNode;
            Assert.IsNotNull(resultAsPvNode);
            Assert.AreNotEqual(resultAsPvNode, _node);
            Assert.AreEqual(9, resultAsPvNode.RealPower, 0.000001);
            Assert.AreEqual(4, resultAsPvNode.VoltageMagnitude, 0.000001);
        }
    }
}
