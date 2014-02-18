using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorHighAccuracyTest :
        LoadFlowCalculatorSmallAccuracyTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndVoltagesAndPowersGivenVersion2_correctResults()
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
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndOnlyOneVoltageGiven_correctResults()
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
    }
}
