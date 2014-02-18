using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using Moq;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    abstract public class LoadFlowCalculatorTest
    {
        abstract protected LoadFlowCalculator CreateLoadFlowCalculator();

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

        [TestMethod]
        public void calculateNodeVoltages_fromOneSideSuppliedConnection_calculatorReceivesCorrectProblem()
        {
            Matrix admittances;
            Node[] nodes;
            double nominalVoltage;
            double expectedOutputVoltage;
            double expectedInputPower;
            CreateOneSideSuppliedConnection(0.1, out admittances, out nodes, out nominalVoltage, out expectedOutputVoltage, out expectedInputPower);
            var admittancesToKnownVoltages = DenseMatrix.OfArray(new[,]{{new Complex(-10, 0)}});
            var admittancesToUnknownVoltages = DenseMatrix.OfArray(new[,]{{new Complex(10, 0)}});
            var knownVoltages = new DenseVector(new[] {new Complex(1, 0)});
            var knownPowers = new DenseVector(new[]{new Complex(-1, 0)});
            var calculatorInternalMock = new Mock<LoadFlowCalculator>();
            calculatorInternalMock
                .Setup(o => o.CalculateNodeVoltagesInternal(admittancesToKnownVoltages, admittancesToUnknownVoltages, nominalVoltage, knownVoltages, knownPowers))
                .Returns(new DenseVector(new[]{new Complex(0.9, 0)}));
            var calculator = calculatorInternalMock.Object;

            var result = calculator.CalculateNodeVoltages(admittances, nominalVoltage, nodes);

            var firstNodeVoltage = result[0].Voltage;
            var firstNodePower = result[0].Power;
            var secondNodeVoltage = result[1].Voltage;
            var secondNodePower = result[1].Power;
            Assert.AreEqual(1, firstNodeVoltage.Real, 0.0001);
            Assert.AreEqual(0, firstNodeVoltage.Imaginary, 0.0001);
            Assert.AreEqual(0.9, secondNodeVoltage.Real, 0.0001);
            Assert.AreEqual(0, secondNodeVoltage.Imaginary, 0.0001);
            Assert.AreEqual(1, firstNodePower.Real, 0.0001);
            Assert.AreEqual(0, firstNodePower.Imaginary, 0.0001);
            Assert.AreEqual(-0.9, secondNodePower.Real, 0.0001);
            Assert.AreEqual(0, secondNodePower.Imaginary, 0.0001);
        }

        private static void CreateOneSideSuppliedConnection(double R, out Matrix admittances, out Node[] nodes, out double nominalVoltage, out double expectedOutputVoltage, out double expectedInputPower)
        {
            double Y = 1.0 / R;
            Complex[,] admittancesArray = { { new Complex(Y, 0), new Complex((-1) * Y, 0) }, { new Complex((-1) * Y, 0), new Complex(Y, 0) } };
            admittances = DenseMatrix.OfArray(admittancesArray);

            var inputNode = new Node();
            var outputNode = new Node();
            inputNode.Voltage = new Complex(1, 0);
            outputNode.Power = new Complex(-1, 0);
            nodes = new[]
            {
                inputNode,
                outputNode
            };

            nominalVoltage = 1;
            expectedOutputVoltage = (1 + Math.Sqrt(1 - 4*R))/2;
            var voltageDifference = 1 - expectedOutputVoltage;
            expectedInputPower = 1 + voltageDifference*voltageDifference/R;
        }
    }
}
