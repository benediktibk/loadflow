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
        protected LoadFlowCalculator _calculator;

        abstract protected LoadFlowCalculator CreateLoadFlowCalculator();

        [TestInitialize]
        public void SetUp()
        {
            _calculator = CreateLoadFlowCalculator();
        }

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_fromOneSideSuppliedConnection_calculatorReceivesCorrectProblemAndCorrectResults()
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
                .Setup(o => o.CalculateUnknownVoltages(admittancesToKnownVoltages, admittancesToUnknownVoltages, nominalVoltage, knownVoltages, knownPowers))
                .Returns(new DenseVector(new[]{new Complex(0.9, 0)}));
            _calculator = calculatorInternalMock.Object;

            nodes = _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);
            
            ComplexAssert.AreEqual(1, 0, nodes[0].Voltage, 0.0001);
            ComplexAssert.AreEqual(0.9, 0, nodes[1].Voltage, 0.0001);
            ComplexAssert.AreEqual(1, 0, nodes[0].Power, 0.0001);
            ComplexAssert.AreEqual(-0.9, 0, nodes[1].Power, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(OverDeterminedProblemException))]
        public void CalculateNodeVoltagesAndPowers_overdeterminedProblem_exceptionThrown()
        {
            var admittances = DenseMatrix.OfArray(
                new [,] {   {new Complex(2, -1),    new Complex(0.1, 0.2)},
                            {new Complex(0.1, 0.2), new Complex(1, -0.5)}});
            var nodes = new[]{new Node(), new Node()};
            nodes[0].Power = new Complex(-1, 2);
            nodes[0].Voltage = new Complex(1, 2);
            nodes[1].Power = new Complex(0.5, -1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, 1, nodes);
        }

        [TestMethod]
        [ExpectedException(typeof(UnderDeterminedProblemException))]
        public void CalculateNodeVoltagesAndPowers_underdeterminedProblem_exceptionThrown()
        {
            var admittances = DenseMatrix.OfArray(
                new[,] {   {new Complex(2, -1),    new Complex(0.1, 0.2)},
                            {new Complex(0.1, 0.2), new Complex(1, -0.5)}});
            var nodes = new[] { new Node(), new Node() };
            nodes[1].Power = new Complex(0.5, -1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, 1, nodes);
        }

        protected static void CreateOneSideSuppliedConnection(double R, out Matrix admittances, out Node[] nodes, out double nominalVoltage, out double expectedOutputVoltage, out double expectedInputPower)
        {
            double Y = 1.0 / R;
            Complex[,] admittancesArray = { { new Complex(Y, 0), new Complex((-1) * Y, 0) }, { new Complex((-1) * Y, 0), new Complex(Y, 0) } };
            admittances = DenseMatrix.OfArray(admittancesArray);

            nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = new Complex(1, 0);
            nodes[1].Power = new Complex(-1, 0);

            nominalVoltage = 1;
            expectedOutputVoltage = (1 + Math.Sqrt(1 - 4*R))/2;
            var voltageDifference = 1 - expectedOutputVoltage;
            expectedInputPower = 1 + voltageDifference*voltageDifference/R;
        }

        protected static void CreateFiveNodeProblem(out Matrix admittances, out Vector voltages, out Vector powers,
            out double nominalVoltage)
        {
            admittances = DenseMatrix.OfArray(new[,]
            {
                {
                    new Complex(120, 20),   new Complex(-100, -50), new Complex(0, 0),      new Complex(-20, 10),   new Complex(0, 20)
                },
                {
                    new Complex(-100, -50), new Complex(110, 80),   new Complex(-10, -30),  new Complex(0, 0),      new Complex(0, 0)
                },
                {
                    new Complex(0, 0),      new Complex(-10, -30),  new Complex(30, -20),   new Complex(-20, 50),   new Complex(0, 0)
                },
                {
                    new Complex(-20, 10),   new Complex(0, 0),      new Complex(-20, 50),   new Complex(110, -10),  new Complex(-70, 50)
                },
                {
                    new Complex(0, 20),     new Complex(0, 0),      new Complex(0, 0),      new Complex(-70, -50),  new Complex(70, 30)
                }
            });

            voltages = new DenseVector(new []{new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0.99, -0.12)});
            var powersCalculated = (voltages.Conjugate()).PointwiseMultiply(admittances.Multiply(voltages));
            powers = new DenseVector(powersCalculated.ToArray());
            nominalVoltage = 1;
        }
    }
}
