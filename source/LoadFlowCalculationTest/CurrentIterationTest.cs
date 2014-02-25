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
        public void CalculateNodeVoltagesAndPowers_OnlyTwoIterationsAllowed_VoltageCollapse()
        {
            var calculator = new CurrentIteration(0.00000001, 2);
            CreateOneSideSuppliedConnection(0.1, out _admittances, out _voltages, out _powers, out _nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = _voltages.At(0);
            nodes[1].Power = _powers.At(1);

            calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            Assert.IsTrue(_voltageCollapse);
        }
    }
}
