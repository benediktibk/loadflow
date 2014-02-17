using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class NodeTest
    {
        [TestMethod]
        public void constructor_idSetTo2_idIs2()
        {
            Node node = new Node(2);

            Assert.AreEqual((uint)2, node.ID);
        }

        [TestMethod]
        [ExpectedException(typeof(VoltageNotSetException))]
        public void getVoltage_notYetSet_exception()
        {
            Node node = new Node(3);

            Complex voltage = node.Voltage;
        }

        [TestMethod]
        [ExpectedException(typeof(PowerNotSetException))]
        public void getPower_notYetSet_exception()
        {
            Node node = new Node(3);

            Complex power = node.Power;
        }

        [TestMethod]
        public void setPower_4And3_powerIs4And3()
        {
            Node node = new Node(3);

            node.Power = new Complex(4, 3);

            Assert.AreEqual(new Complex(4, 3), node.Power);
        }

        [TestMethod]
        public void setVoltage_2And4_voltageIs2And4()
        {
            Node node = new Node(45);

            node.Voltage = new Complex(2, 4);

            Assert.AreEqual(new Complex(2, 4), node.Voltage);
        }
    }
}
