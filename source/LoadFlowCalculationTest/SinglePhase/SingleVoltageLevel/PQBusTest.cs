using System;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PQBusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeID_ThrowsException()
        {
            var bus = new PQBus(-2, new Complex());
        }

        [TestMethod]
        public void Constructor_5AsID_IDIs5()
        {
            var bus = new PQBus(5, new Complex(4, 3));

            Assert.AreEqual(5, bus.ID);
        }

        [TestMethod]
        public void Constructor_4And3AsPower_PowerIs4And4()
        {
            var bus = new PQBus(5, new Complex(4, 3));

            Assert.AreEqual(new Complex(4, 3), bus.Power);
        }
    }
}
