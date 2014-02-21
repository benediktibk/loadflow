using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;
using Complex = System.Numerics.Complex;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorSmallAccuracyTest :
        LoadFlowCalculatorTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionWithBigResistance_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.1, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] {new Node(), new Node()};
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.1, 0.2);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionWithSmallResistance_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionAndOnlyVoltagesKnown_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Voltage = voltages.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedAndInverseInformationGiven_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Voltage = voltages.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndOnlyVoltagesGiven_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateFiveNodeProblem(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Voltage = voltages.At(1);
            nodes[2].Voltage = voltages.At(2);
            nodes[3].Voltage = voltages.At(3);
            nodes[4].Voltage = voltages.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.0001);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndVoltagesAndPowersGiven_correctResults()
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
            
            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            NodeAssert.AreEqual(nodes, voltages, powers, 0.3, 400);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_threeNodeProblemAndTwoVoltagesGiven_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateThreeNodeProblemWithGroundNode(out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].Voltage = voltages.At(2);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            // very small differences in the voltages cause already very big errors in the load flow, therefore the load flow is not very accurate with the node potential method
            NodeAssert.AreEqual(nodes, voltages, powers, 0.3, 1000);
        }
    }
}
