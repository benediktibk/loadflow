using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Complex = System.Numerics.Complex;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorHighAccuracyTest :
        LoadFlowCalculatorSmallAccuracyTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndVoltagesAndPowersGivenVersionTwo_CorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Voltage = voltages.At(1);
            nodes[2].Voltage = voltages.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionOne_CorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Voltage = voltages.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Power = powers.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemAndOnlyOneVoltageGivenVersionTwo_CorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Power = powers.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.01);
        }
        
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_ThreeNodeProblemAndTwoVoltagesGivenVersionTwo_CorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateThreeNodeProblemWithGroundNode(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Voltage = voltages.At(1);
            nodes[2].Voltage = voltages.At(2);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 10);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_FiveNodeProblemWithGroundNodeVersionTwo_CorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateFiveNodeProblemWithGroundNode(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Power = powers.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = voltages.At(4);

            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 1);
        }
    }
}
