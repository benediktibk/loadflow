using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class LoadFlowCalculatorTest
    {
        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnection_correctVoltages()
        {
            const double R = 0.1;
            Complex[,] admittancesArray = { { new Complex(R, 0), new Complex((-1)*R, 0) }, { new Complex((-1)*R, 0), new Complex(R, 0) } };
            var admittances = DenseMatrix.OfArray(admittancesArray);
            var inputNode = new Node();
            var outputNode = new Node();
            inputNode.Voltage = new Complex(1, 0);
            outputNode.Power = new Complex(1, 0);
            var nodes = new Node[2]
            {
                inputNode,
                outputNode
            };
            LoadFlowCalculator caclulator = new NodePotentialMethod();

            nodes = caclulator.CalculateNodeVoltages(admittances, 1, nodes);

            inputNode = nodes[0];
            outputNode = nodes[1];
            Complex inputVoltage = inputNode.Voltage;
            Complex outputVoltage = outputNode.Voltage;
            Complex inputPower = inputNode.Power;
            Complex outputPower = outputNode.Power;
            Assert.AreEqual(1, inputVoltage.Real, 0.0001);
            Assert.AreEqual(0, inputVoltage.Imaginary, 0.0001);
            Assert.AreEqual((1 + Math.Sqrt(0.6))/2, outputVoltage.Real, 0.0001);
            Assert.AreEqual(0, outputVoltage.Imaginary, 0.0001);
            Assert.AreEqual(1.127016654, inputPower.Real, 0.0001);
            Assert.AreEqual(0, inputPower.Imaginary, 0.0001);
            Assert.AreEqual(1, outputPower.Real, 0.0001);
            Assert.AreEqual(0, outputPower.Imaginary, 0.0001);
        }
    }
}
