using System;
using Calculation.SinglePhase.SingleVoltageLevel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class PowerNetTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NominalVoltage0_ThrowsException()
        {
            var powerNet = new PowerNet(new AdmittanceMatrix(2), 0);
        }

        [TestMethod]
        public void Constructor_AdmittanceMatrixWithThreeNodes_NodeCountIs3()
        {
            var powerNet = new PowerNet(new AdmittanceMatrix(3), 2);

            Assert.AreEqual(3, powerNet.NodeCount);
        }

        [TestMethod]
        public void Constructor_NominalVoltage2_NominalVoltageIs2()
        {
            var powerNet = new PowerNet(new AdmittanceMatrix(3), 2);

            Assert.AreEqual(2, powerNet.NominalVoltage);
        }

        [TestMethod]
        public void AdmittanceMatrix_Empty_SameObjectAsConstructorGot()
        {
            var admittanceMatrix = new AdmittanceMatrix(3);
            var powerNet = new PowerNet(admittanceMatrix, 2);

            Assert.AreEqual(admittanceMatrix, powerNet.Admittances);
        }
    }
}
