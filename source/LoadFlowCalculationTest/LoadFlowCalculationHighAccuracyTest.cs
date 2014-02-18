using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculationHighAccuracyTest :
        LoadFlowCalculatorSmallAccuracyTest
    {
        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnectionAndOnlyPowersKnown_correctResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.0001, 0.01);
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fiveNodeProblemAndOnlyPowersGiven_correctResults()
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
            nodes[4].Power = powers.At(4);

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);

            NodeAssert.AreEqual(nodes, voltages, powers, 0.2, 2);
        }
    }
}
