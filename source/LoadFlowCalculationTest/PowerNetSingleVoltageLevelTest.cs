using System;
using System.Numerics;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class PowerNetSingleVoltageLevelTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NodeCount0_ThrowsException()
        {
            var powerNet = new PowerNetSingleVoltageLevel(0, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NominalVoltage0_ThrowsException()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 0);
        }

        [TestMethod]
        public void Constructor_NodeCount3_NodeCountIs3()
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 2);

            Assert.AreEqual(3, powerNet.NodeCount);
        }

        [TestMethod]
        public void Constructor_NominalVoltage2_NominalVoltageIs2()
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 2);

            Assert.AreEqual(2, powerNet.NominalVoltage);
        }

        [TestMethod]
        public void CalculateMissingInformation_TwoNodeSystem_CorrectVoltagesAndPowers()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            var supplyNode = new Node();
            var loadNode = new Node();
            const double admittance = 100;
            const double load = 0.1;
            supplyNode.Voltage = new Complex(1, 0);
            loadNode.Power = new Complex((-1)*load, 0);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(admittance, 0));

            var voltageCollapse = powerNet.CalculateMissingInformation(new LoadFlowCalculator(new CurrentIteration(0.000000001, 1000000)));

            var voltages = powerNet.NodeVoltages;
            var powers = powerNet.NodePowers;
            Assert.IsFalse(voltageCollapse);
            Assert.AreEqual(2, voltages.Count);
            Assert.AreEqual(2, powers.Count);
            var loadVoltage = (1 + Math.Sqrt(1 - 4*load/admittance))/2;
            ComplexAssert.AreEqual(1, 0, voltages[0], 0.0001);
            ComplexAssert.AreEqual(loadVoltage, 0, voltages[1], 0.0001);
            ComplexAssert.AreEqual((1 - loadVoltage) * admittance, 0, powers[0], 0.0001);
            ComplexAssert.AreEqual(-0.1, 0, powers[1], 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddSymmetricAdmittance_DiagonalPosition_ThrowsException()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);

            powerNet.AddSymmetricAdmittance(1, 1, new Complex());
        }

        [TestMethod]
        public void AddSymmetricAdmittance_OnePositionTwiceSet_AllValuesAreCorrect()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);

            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, -2));
            powerNet.AddSymmetricAdmittance(1, 0, new Complex(3, 1));

            ComplexAssert.AreEqual(4, -1, powerNet.GetAdmittance(0, 0), 0.0001);
            ComplexAssert.AreEqual(4, -1, powerNet.GetAdmittance(1, 1), 0.0001);
            ComplexAssert.AreEqual(-4, 1, powerNet.GetAdmittance(1, 0), 0.0001);
            ComplexAssert.AreEqual(-4, 1, powerNet.GetAdmittance(0, 1), 0.0001);
        }

        [TestMethod]
        public void AddUnsymmetricAdmittance_SymmetricValuesAlreadySet_AllValuesAreCorrect()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);

            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, -2));
            powerNet.AddUnsymmetricAdmittance(1, 0, new Complex(3, 1));

            ComplexAssert.AreEqual(1, -2, powerNet.GetAdmittance(0, 0), 0.0001);
            ComplexAssert.AreEqual(1, -2, powerNet.GetAdmittance(1, 1), 0.0001);
            ComplexAssert.AreEqual(-1, 2, powerNet.GetAdmittance(0, 1), 0.0001);
            ComplexAssert.AreEqual(2, 3, powerNet.GetAdmittance(1, 0), 0.0001);
        }

        [TestMethod]
        public void RelativePowerError_StableTwoNodeSystem_0()
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            var supplyNode = new Node();
            var loadNode = new Node();
            const double admittance = 100;
            const double load = 0.1;
            supplyNode.Voltage = new Complex(1, 0);
            loadNode.Power = new Complex((-1) * load, 0);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(admittance, 0));
            powerNet.CalculateMissingInformation(new LoadFlowCalculator(new CurrentIteration(0.000000001, 1000000)));

            var relativePowerError = powerNet.RelativePowerError;

            Assert.AreEqual(0, relativePowerError , 0.0000001);
        }

        [TestMethod]
        public void RelativePowerError_CollapsingSystem_Not0()
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            var supplyNode = new Node();
            var loadNodeOne = new Node();
            var loadNodeTwo = new Node();
            supplyNode.Voltage = new Complex(1, 0);
            loadNodeOne.Power = new Complex(-100, 0);
            loadNodeTwo.Power = new Complex(-100, 0);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNodeOne);
            powerNet.SetNode(2, loadNodeTwo);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, 0));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(2, 0));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(3, 0));
            var voltageCollapse = powerNet.CalculateMissingInformation(new LoadFlowCalculator(new CurrentIteration(0.000001, 1)));

            var relativePowerError = powerNet.RelativePowerError;

            Assert.IsTrue(voltageCollapse);
            Assert.AreNotEqual(0, relativePowerError, 0.0000001);
        }

        [TestMethod]
        public void CalculateMissingInformation_CollapsingSystem_InitalSetValuesAreCorrect()
        {
            var powerNet = new PowerNetSingleVoltageLevel(3, 1);
            var supplyNode = new Node();
            var loadNodeOne = new Node();
            var loadNodeTwo = new Node();
            supplyNode.Voltage = new Complex(1, 0);
            loadNodeOne.Power = new Complex(-100, 0);
            loadNodeTwo.Power = new Complex(-200, 0);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNodeOne);
            powerNet.SetNode(2, loadNodeTwo);
            powerNet.AddSymmetricAdmittance(0, 1, new Complex(1, 0));
            powerNet.AddSymmetricAdmittance(0, 2, new Complex(2, 0));
            powerNet.AddSymmetricAdmittance(1, 2, new Complex(3, 0));

            var voltageCollapse = powerNet.CalculateMissingInformation(new LoadFlowCalculator(new CurrentIteration(0.000001, 1)));

            var voltages = powerNet.NodeVoltages;
            var powers = powerNet.NodePowers;
            Assert.IsTrue(voltageCollapse);
            ComplexAssert.AreEqual(1, 0, voltages[0], 0.000001);
            ComplexAssert.AreEqual(-100, 0, powers[1], 0.000001);
            ComplexAssert.AreEqual(-200, 0, powers[2], 0.000001);
        }
    }
}
