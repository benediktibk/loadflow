using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;
using Complex = System.Numerics.Complex;

namespace LoadFlowCalculationTest
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
        public void getVoltage_NotYetSet_ThrowsException()
        {
            Complex voltage = _node.Voltage;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void getPower_NotYetSet_ThrowsException()
        {
            Complex power = _node.Power;
        }

        [TestMethod]
        public void setPower_4And3_PowerIs4And3()
        {
            _node.Power = new Complex(4, 3);

            Assert.AreEqual(new Complex(4, 3), _node.Power);
        }

        [TestMethod]
        public void setVoltage_2And4_VoltageIs2And4()
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
    }
}
