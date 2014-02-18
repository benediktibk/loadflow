using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorSmallAccuracyTest :
        LoadFlowCalculatorTest
    {
        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnectionWithBigResistance_correctVoltages()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.1, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            LoadFlowCalculator caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltages(admittances, nominalVoltage, nodes);

            var inputNode = nodes[0];
            var outputNode = nodes[1];
            var inputVoltage = inputNode.Voltage;
            var outputVoltage = outputNode.Voltage;
            var inputPower = inputNode.Power;
            var outputPower = outputNode.Power;
            Assert.AreEqual(1, inputVoltage.Real, 0.0001);
            Assert.AreEqual(0, inputVoltage.Imaginary, 0.0001);
            Assert.AreEqual(expectedOutputVoltage, outputVoltage.Real, 0.1);
            Assert.AreEqual(0, outputVoltage.Imaginary, 0.0001);
            Assert.AreEqual(expectedInputPower, inputPower.Real, 0.2);
            Assert.AreEqual(0, inputPower.Imaginary, 0.0001);
            Assert.AreEqual(-1, outputPower.Real, 0.2);
            Assert.AreEqual(0, outputPower.Imaginary, 0.0001);
        }

        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnectionWithSmallResistance_correctVoltages()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.001, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            LoadFlowCalculator caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltages(admittances, nominalVoltage, nodes);

            var inputNode = nodes[0];
            var outputNode = nodes[1];
            var inputVoltage = inputNode.Voltage;
            var outputVoltage = outputNode.Voltage;
            var inputPower = inputNode.Power;
            var outputPower = outputNode.Power;
            Assert.AreEqual(1, inputVoltage.Real, 0.0001);
            Assert.AreEqual(0, inputVoltage.Imaginary, 0.0001);
            Assert.AreEqual(expectedOutputVoltage, outputVoltage.Real, 0.0001);
            Assert.AreEqual(0, outputVoltage.Imaginary, 0.0001);
            Assert.AreEqual(expectedInputPower, inputPower.Real, 0.01);
            Assert.AreEqual(0, inputPower.Imaginary, 0.0001);
            Assert.AreEqual(-1, outputPower.Real, 0.01);
            Assert.AreEqual(0, outputPower.Imaginary, 0.0001);
        }
    }
}
