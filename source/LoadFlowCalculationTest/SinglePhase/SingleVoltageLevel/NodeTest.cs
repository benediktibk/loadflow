using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class NodeTest
    {
        private Node _node;

        [TestInitialize]
        public void SetUp()
        {
            _node = new Node();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetVoltage_NotYetSet_ThrowsException()
        {
            var voltage = _node.Voltage;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetPower_NotYetSet_ThrowsException()
        {
            var power = _node.Power;
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void GetVoltageMagnitude_NotYetSet_ThrowsException()
        {
            var voltageMagnitude = _node.VoltageMagnitude;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRealPower_NotYetSet_ThrowsException()
        {
            var realPower = _node.RealPower;
        }

        [TestMethod]
        public void SetPower_4And3_PowerIs4And3()
        {
            _node.Power = new Complex(4, 3);

            Assert.AreEqual(new Complex(4, 3), _node.Power);
        }

        [TestMethod]
        public void SetVoltage_2And4_VoltageIs2And4()
        {
            _node.Voltage = new Complex(2, 4);

            Assert.AreEqual(new Complex(2, 4), _node.Voltage);
        }

        [TestMethod]
        public void VoltageIsKnown_VoltageNotSet_False()
        {
            Assert.IsFalse(_node.VoltageIsKnown);
        }

        [TestMethod]
        public void VoltageIsKnown_VoltageSet_True()
        {
            _node.Voltage = new Complex(0, 2);

            Assert.IsTrue(_node.VoltageIsKnown);
        }

        [TestMethod]
        public void PowerIsKnown_PowerIsNotSet_False()
        {
            Assert.IsFalse(_node.PowerIsKnown);
        }

        [TestMethod]
        public void PowerIsKnown_PowerIsSet_True()
        {
            _node.Power = new Complex(2, 2);

            Assert.IsTrue(_node.PowerIsKnown);
        }

        [TestMethod]
        public void RealPowerIsKnown_RealPowerIsNotSet_False()
        {
            Assert.IsFalse(_node.RealPowerIsKnown);
        }

        [TestMethod]
        public void RealPowerIsKnown_RealPowerIsSet_True()
        {
            _node.RealPower = 3;

            Assert.IsTrue(_node.RealPowerIsKnown);
        }

        [TestMethod]
        public void SetRealPower_PowerPreviouslySet_PowerIsNotKnown()
        {
            _node.Power = new Complex(1, 2);
            _node.RealPower = 4;

            Assert.IsFalse(_node.PowerIsKnown);
        }

        [TestMethod]
        public void SetPower_RealPowerPreviouslySet_RealPowerIsKnown()
        {
            _node.RealPower = 4;
            _node.Power = new Complex(1, 2);

            Assert.IsTrue(_node.RealPowerIsKnown);
        }

        [TestMethod]
        public void SetVoltageMagnitude_VoltagePreviouslySet_VoltageIsNotKnown()
        {
            _node.Voltage = new Complex(1, 2);
            _node.VoltageMagnitude = 4;

            Assert.IsFalse(_node.VoltageIsKnown);
        }

        [TestMethod]
        public void SetVoltage_VoltageMagnitudePreviouslySet_VoltageMagnitudeIsKnown()
        {
            _node.VoltageMagnitude = 4;
            _node.Voltage = new Complex(1, 2);

            Assert.IsTrue(_node.VoltageMagnitudeIsKnown);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void SetVoltageMagnitude_NegativeValue_ThrowsException()
        {
            _node.VoltageMagnitude = -3;
        }

        [TestMethod]
        public void GetVoltageMagnitude_VoltageMagnitudeSetTo2_2()
        {
            _node.VoltageMagnitude = 2;

            Assert.AreEqual(2, _node.VoltageMagnitude);
        }

        [TestMethod]
        public void GetRealPower_RealPowerSetTo1_1()
        {
            _node.RealPower = 1;

            Assert.AreEqual(1, _node.RealPower);
        }

        [TestMethod]
        public void GetVoltageMagnitude_VoltageSetTo3And4_5()
        {
            _node.Voltage = new Complex(3, 4);

            Assert.AreEqual(5, _node.VoltageMagnitude, 0.0001);
        }

        [TestMethod]
        public void GetRealPower_PowerSetTo4And5_4()
        {
            _node.Power = new Complex(4, 5);

            Assert.AreEqual(4, _node.RealPower, 0.0001);
        }

        [TestMethod]
        public void IsPQBus_OnlyPowerSet_True()
        {
            _node.Power = new Complex(3, 3);

            Assert.IsTrue(_node.IsPQBus);
        }

        [TestMethod]
        public void IsPQBus_OnlyRealPowerSet_False()
        {
            _node.RealPower = 3;

            Assert.IsFalse(_node.IsPQBus);
        }

        [TestMethod]
        public void IsPQBus_PowerAndVoltageSet_False()
        {
            _node.Power = new Complex(3, 3);
            _node.Voltage = new Complex(2, 3);

            Assert.IsFalse(_node.IsPQBus);
        }

        [TestMethod]
        public void IsPQBus_PowerAndVoltageMagnitudeSet_False()
        {
            _node.Power = new Complex(3, 3);
            _node.VoltageMagnitude = 1;

            Assert.IsFalse(_node.IsPQBus);
        }

        [TestMethod]
        public void IsSlackBus_OnlyVoltageSet_True()
        {
            _node.Voltage = new Complex(2, 3);

            Assert.IsTrue(_node.IsSlackBus);
        }

        [TestMethod]
        public void IsSlackBus_OnlyVoltageMagnitudeSet_False()
        {
            _node.VoltageMagnitude = 3;

            Assert.IsFalse(_node.IsSlackBus);
        }

        [TestMethod]
        public void IsSlackBus_VoltageAndPowerSet_False()
        {
            _node.Voltage = new Complex(2, 3);
            _node.Power = new Complex(3, 3);

            Assert.IsFalse(_node.IsSlackBus);
        }

        [TestMethod]
        public void IsPVBus_RealPowerAndVoltageMagnitudeSet_True()
        {
            _node.VoltageMagnitude = 3;
            _node.RealPower = 1;

            Assert.IsTrue(_node.IsPVBus);
        }

        [TestMethod]
        public void IsPVBus_OnlyVoltageMagnitudeSet_False()
        {
            _node.VoltageMagnitude = 3;

            Assert.IsFalse(_node.IsPVBus);
        }

        [TestMethod]
        public void IsPVBus_OnlyRealPowerSet_False()
        {
            _node.RealPower = 1;

            Assert.IsFalse(_node.IsPVBus);
        }

        [TestMethod]
        public void IsPVBus_PowerAndVoltageMagnitudeSet_False()
        {
            _node.VoltageMagnitude = 3;
            _node.Power = new Complex(3, 3);

            Assert.IsFalse(_node.IsPVBus);
        }

        [TestMethod]
        public void IsPVBus_RealPowerAndVoltageSet_False()
        {
            _node.RealPower = 1;
            _node.Voltage = new Complex(2, 3);

            Assert.IsFalse(_node.IsPVBus);
        }
    }
}
