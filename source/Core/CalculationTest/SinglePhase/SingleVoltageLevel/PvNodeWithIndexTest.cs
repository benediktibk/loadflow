using System;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PvNodeWithIndexTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeID_ThrowsException()
        {
            var bus = new PvNodeWithIndex(-2, 3, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeVoltageMagnitude_ThrowsException()
        {
            var bus = new PvNodeWithIndex(0, 3, -1);
        }

        [TestMethod]
        public void Constructor_IDSetTo1_IDIs1()
        {
            var bus = new PvNodeWithIndex(1, 2, 3);

            Assert.AreEqual(1, bus.Index);
        }

        [TestMethod]
        public void Constructor_RealPowerSetTo2_RealPowerIs2()
        {
            var bus = new PvNodeWithIndex(1, 2, 3);

            Assert.AreEqual(2, bus.RealPower);
        }

        [TestMethod]
        public void Constructor_VoltageMagnitudeSetTo3_VoltageMagnitudeIs3()
        {
            var bus = new PvNodeWithIndex(1, 2, 3);

            Assert.AreEqual(3, bus.VoltageMagnitude);
        }
    }
}
