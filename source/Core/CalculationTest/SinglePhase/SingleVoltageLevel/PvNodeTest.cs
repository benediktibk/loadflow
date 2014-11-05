using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
        public void SetVoltageIn_NullAndValidId_SetsNoValue()
        {
            _node.SetVoltageIn(null, 1);
        }

        [TestMethod]
        public void SetVoltageMagnitudeIn_NullAndValidId_CorrectMagnitudeSet()
        {
            var voltages = new DenseVector(new []{new Complex(), Complex.FromPolarCoordinates(5, 6), new Complex()});

            _node.SetVoltageMagnitudeIn(voltages, 1);

            ComplexAssert.AreEqual(new Complex(), voltages[0], 0.00001);
            ComplexAssert.AreEqual(Complex.FromPolarCoordinates(4, 6), voltages[1], 0.00001);
            ComplexAssert.AreEqual(new Complex(), voltages[2], 0.00001);
        }

        [TestMethod]
        public void SetRealPowerIn_NullAndValidId_CorrectRealPowerSet()
        {
            var powers = new DenseVector(new[] { new Complex(1, 2), new Complex(3, 4), new Complex(5, 6) });

            _node.SetRealPowerIn(powers, 0);

            ComplexAssert.AreEqual(3, 2, powers[0], 0.00001);
            ComplexAssert.AreEqual(3, 4, powers[1], 0.00001);
            ComplexAssert.AreEqual(5, 6, powers[2], 0.00001);
        }

        [TestMethod]
        public void SetPowerIn_NullAndValidId_SetsNoValue()
        {
            _node.SetPowerIn(null, 2);
        }
    }
}
