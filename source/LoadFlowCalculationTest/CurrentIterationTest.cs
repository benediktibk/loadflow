using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class CurrentIterationTest :
        LoadFlowCalculatorHighAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new CurrentIteration(0.00001, 1000);
        }

        [TestMethod]
        [ExpectedException(typeof(NotConvergingException))]
        public void CalculateNodeVoltagesAndPowers_onlyTwoIterationsAllowed_throwsException()
        {
            var calculator = new CurrentIteration(0.00000001, 2);
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.1, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);

            calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);
        }
    }
}
