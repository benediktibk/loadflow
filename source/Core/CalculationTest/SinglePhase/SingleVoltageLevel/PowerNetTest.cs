using System;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

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
        [TestCategory("Integration")]
        public void CalculateMissingInformation_TwoNodeSystem_CorrectVoltagesAndPowers()
        {
            var admittanceMatrix = new AdmittanceMatrix(2);
            var supplyNode = new Node();
            var loadNode = new Node();
            const double admittance = 100;
            const double load = 0.1;
            supplyNode.Voltage = new Complex(1, 0);
            loadNode.Power = new Complex((-1)*load, 0);
            admittanceMatrix.AddConnection(0, 1, new Complex(admittance, 0));
            var nodeVoltageCalculator = new CurrentIteration(0.000000001, 1000000);
            var powerNet = new PowerNetComputable(nodeVoltageCalculator, admittanceMatrix, 1);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);

            var nodeResults = powerNet.CalculateNodeResults();

            Assert.IsNotNull(nodeResults);
            Assert.AreEqual(2, nodeResults.Count);
            var loadVoltage = (1 + Math.Sqrt(1 - 4*load/admittance))/2;
            ComplexAssert.AreEqual(1, 0, nodeResults[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(loadVoltage, 0, nodeResults[1].Voltage, 0.0001);
            ComplexAssert.AreEqual((1 - loadVoltage) * admittance, 0, nodeResults[0].Power, 0.0001);
            ComplexAssert.AreEqual(-0.1, 0, nodeResults[1].Power, 0.0001);
        }
    }
}
