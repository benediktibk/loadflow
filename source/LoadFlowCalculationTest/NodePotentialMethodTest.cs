using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class NodePotentialMethodTest :
        LoadFlowCalculatorSmallAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new NodePotentialMethod();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFullRankException))]
        public void CalculateNodeVoltagesAndPowers_onlyPowersKnown_throwsException()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Power = powers.At(1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);
        }
    }
}
