using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PqBusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeID_ThrowsException()
        {
            var bus = new PqBus(-2, new Complex());
        }

        [TestMethod]
        public void Constructor_5AsID_IDIs5()
        {
            var bus = new PqBus(5, new Complex(4, 3));

            Assert.AreEqual(5, bus.Id);
        }

        [TestMethod]
        public void Constructor_4And3AsPower_PowerIs4And4()
        {
            var bus = new PqBus(5, new Complex(4, 3));

            Assert.AreEqual(new Complex(4, 3), bus.Power);
        }
    }
}
