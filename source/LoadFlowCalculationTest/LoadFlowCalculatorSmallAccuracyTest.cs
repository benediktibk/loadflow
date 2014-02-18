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
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionWithBigResistance_correctResults()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.1, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            var caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            ComplexAssert.AreEqual(1, 0, nodes[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedOutputVoltage, 0, nodes[1].Voltage, 0.1);
            ComplexAssert.AreEqual(expectedInputPower, 0, nodes[0].Power, 0.2);
            ComplexAssert.AreEqual(-1, 0, nodes[1].Power, 0.2);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionWithSmallResistance_correctResults()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.001, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            var caclulator = CreateLoadFlowCalculator();

            nodes = caclulator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);
            
            ComplexAssert.AreEqual(1, 0, nodes[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedOutputVoltage, 0, nodes[1].Voltage, 0.0001);
            ComplexAssert.AreEqual(expectedInputPower, 0, nodes[0].Power, 0.01);
            ComplexAssert.AreEqual(-1, 0, nodes[1].Power, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndOnlyPowersGiven_correctResults()
        {
            Matrix admittances;
            Vector voltages;
            Vector powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var calculator = CreateLoadFlowCalculator();
            var nodes = new[] {new Node(), new Node(), new Node(), new Node(), new Node()};
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Power = powers.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Power = powers.At(4);

            nodes = calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndOnlyVoltagesGiven_correctResults()
        {
            Matrix admittances;
            Vector voltages;
            Vector powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var calculator = CreateLoadFlowCalculator();
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Voltage = voltages.At(1);
            nodes[2].Voltage = voltages.At(2);
            nodes[3].Voltage = voltages.At(3);
            nodes[4].Voltage = voltages.At(4);

            nodes = calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.05);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndVoltagesAndPowersGiven_correctResults()
        {
            Matrix admittances;
            Vector voltages;
            Vector powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var calculator = CreateLoadFlowCalculator();
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Voltage = voltages.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = voltages.At(4);

            nodes = calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.05);
        }
    }
}
