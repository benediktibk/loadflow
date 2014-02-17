using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

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
        [ExpectedException(typeof(VoltageNotSetException))]
        public void getVoltage_notYetSet_exception()
        {
            Complex voltage = _node.Voltage;
        }

        [TestMethod]
        [ExpectedException(typeof(PowerNotSetException))]
        public void getPower_notYetSet_exception()
        {
            Complex power = _node.Power;
        }

        [TestMethod]
        public void setPower_4And3_powerIs4And3()
        {
            _node.Power = new Complex(4, 3);

            Assert.AreEqual(new Complex(4, 3), _node.Power);
        }

        [TestMethod]
        public void setVoltage_2And4_voltageIs2And4()
        {
            _node.Voltage = new Complex(2, 4);

            Assert.AreEqual(new Complex(2, 4), _node.Voltage);
        }

        [TestMethod]
        public void VoltageIsKnown_voltageNotSet_false()
        {
            Assert.IsFalse(_node.VoltageIsKnown);
        }

        [TestMethod]
        public void VoltageIsKnown_voltageSet_true()
        {
            _node.Voltage = new Complex(0, 2);

            Assert.IsTrue(_node.VoltageIsKnown);
        }

        [TestMethod]
        public void PowerIsKnown_powerIsNotSet_false()
        {
            Assert.IsFalse(_node.PowerIsKnown);
        }

        [TestMethod]
        public void PowerIsKnown_powerIsSet_true()
        {
            _node.Power = new Complex(2, 2);

            Assert.IsTrue(_node.PowerIsKnown);
        }
    }
}
