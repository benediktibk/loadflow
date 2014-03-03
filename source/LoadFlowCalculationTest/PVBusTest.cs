using System;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class PVBusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeID_ThrowsException()
        {
            var bus = new PVBus(-2, 3, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeVoltageMagnitude_ThrowsException()
        {
            var bus = new PVBus(0, 3, -1);
        }

        [TestMethod]
        public void Constructor_IDSetTo1_IDIs1()
        {
            var bus = new PVBus(1, 2, 3);

            Assert.AreEqual(1, bus.ID);
        }

        [TestMethod]
        public void Constructor_RealPowerSetTo2_RealPowerIs2()
        {
            var bus = new PVBus(1, 2, 3);

            Assert.AreEqual(2, bus.RealPower);
        }

        [TestMethod]
        public void Constructor_VoltageMagnitudeSetTo3_VoltageMagnitudeIs3()
        {
            var bus = new PVBus(1, 2, 3);

            Assert.AreEqual(3, bus.VoltageMagnitude);
        }
    }
}
