using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Complex;
using Moq;
using LoadFlowCalculation;
using Complex = System.Numerics.Complex;

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
        public void CalculateNodeVoltagesAndPowers_FromOneSideSuppliedConnection_CalculatorReceivesCorrectProblemAndCorrectResults()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.1, out admittances, out voltages, out powers, out nominalVoltage);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);
            var admittancesToKnownVoltages = DenseMatrix.OfArray(new[,]{{new Complex(-10, 0)}});
            var admittancesToUnknownVoltages = DenseMatrix.OfArray(new[,]{{new Complex(10, 0)}});
            var knownVoltages = new DenseVector(new[] {new Complex(1, 0)});
            var knownPowers = new DenseVector(new[]{new Complex(-1, 0)});
            var constantCurrents = admittancesToKnownVoltages.Multiply(knownVoltages).Multiply(new Complex(-1, 0));
            var calculatorInternalMock = new Mock<LoadFlowCalculator>();
            calculatorInternalMock
                .Setup(o => o.CalculateUnknownVoltages(admittancesToUnknownVoltages, nominalVoltage, constantCurrents, knownPowers))
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
        public void CalculateNodeVoltagesAndPowers_OverdeterminedProblem_ExceptionThrown()
        {
            var admittances = DenseMatrix.OfArray(
                new [,] {   {new Complex(2, -1),    new Complex(-2, 1)},
                            {new Complex(-2, 1), new Complex(2, -1)}});
            var nodes = new[]{new Node(), new Node()};
            nodes[0].Power = new Complex(-1, 2);
            nodes[0].Voltage = new Complex(1, 2);
            nodes[1].Power = new Complex(0.5, -1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, 1, nodes);
        }

        [TestMethod]
        [ExpectedException(typeof(UnderDeterminedProblemException))]
        public void CalculateNodeVoltagesAndPowers_UnderdeterminedProblem_ExceptionThrown()
        {
            var admittances = DenseMatrix.OfArray(
                new[,] {   {new Complex(2, -1),    new Complex(-2, 1)},
                            {new Complex(-2, 1), new Complex(2, -1)}});
            var nodes = new[] { new Node(), new Node() };
            nodes[1].Power = new Complex(0.5, -1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, 1, nodes);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSymmetricException))]
        public void CalculateNodeVoltagesAndPowers_NotSymmetricAdmittanceMatrix_ExceptionThrown()
        {
            var admittances = DenseMatrix.OfArray(
                new[,] {   {new Complex(2, -1),    new Complex(0.1, 0.2)},
                            {new Complex(0, 0.2), new Complex(1, -0.5)}});
            var nodes = new[] { new Node(), new Node() };
            nodes[1].Power = new Complex(0.5, -1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, 1, nodes);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFullRankException))]
        public void CalculateNodeVoltagesAndPowers_OnlyPowersKnown_ThrowsException()
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

        [TestMethod]
        [ExpectedException(typeof (InvalidAdmittanceMatrixException))]
        public void CalculateNodeVoltagesAndPowers_InvalidAdmittanceMatrix_ThrowsException()
        {
            Matrix<Complex> admittances;
            Vector<Complex> voltages;
            Vector<Complex> powers;
            double nominalVoltage;
            CreateOneSideSuppliedConnection(0.001, out admittances, out voltages, out powers, out nominalVoltage);
            var diagonal = admittances.Diagonal();
            var diagonalArray = diagonal.ToArray();
            diagonalArray[0] += new Complex(1, 0);
            diagonal.SetValues(diagonalArray);
            admittances.SetDiagonal(diagonal);
            var nodes = new[] { new Node(), new Node() };
            nodes[0].Voltage = voltages.At(0);
            nodes[1].Power = powers.At(1);

            _calculator.CalculateNodeVoltagesAndPowers(admittances, nominalVoltage, nodes);
        }

        protected static void CreateOneSideSuppliedConnection(double R, out Matrix<Complex> admittances, out Vector<Complex> voltages, out Vector<Complex> powers, out double nominalVoltage)
        {
            double Y = 1.0 / R;
            Complex[,] admittancesArray = { { new Complex(Y, 0), new Complex((-1) * Y, 0) }, { new Complex((-1) * Y, 0), new Complex(Y, 0) } };
            admittances = DenseMatrix.OfArray(admittancesArray);

            var inputVoltage = new Complex(1, 0);
            var outputVoltage = new Complex((1 + Math.Sqrt(1 - 4*R))/2, 0);
            var voltageDifference = 1 - outputVoltage.Real;
            var inputPower = new Complex(1 + voltageDifference*voltageDifference/R, 0);
            var outputPower = new Complex(-1, 0);
            voltages = new DenseVector(new []{inputVoltage, outputVoltage});
            powers = new DenseVector(new []{inputPower, outputPower});
            nominalVoltage = 1;
        }

        protected static void CreateFiveNodeProblem(out Matrix<Complex> admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateFiveNodeProblemAdmittanceMatrix(
                new Complex(1000, 500), new Complex(0, 0), new Complex(200, -100), new Complex(0, -200),
                new Complex(100, 300), new Complex(0, 0), new Complex(0, 0),
                new Complex(200, -500), new Complex(0, 0),
                new Complex(700, 500));

            voltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0.99, -0.12) });
            var currents = admittances.Multiply(voltages);
            powers = voltages.PointwiseMultiply(currents.Conjugate());
            nominalVoltage = 1;
        }

        protected static void CreateFiveNodeProblemWithGroundNode(out Matrix<Complex> admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateFiveNodeProblemAdmittanceMatrix(
                new Complex(1000, 500), new Complex(0, 0), new Complex(200, -100), new Complex(50, -20),
                new Complex(100, 300), new Complex(0, 0), new Complex(0, 0),
                new Complex(200, -500), new Complex(0, 0),
                new Complex(10, -5));

            voltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0, 0) });
            var currents = admittances.Multiply(voltages);
            powers = voltages.PointwiseMultiply(currents.Conjugate());
            nominalVoltage = 1;
        }

        protected static Matrix<Complex> CreateFiveNodeProblemAdmittanceMatrix(Complex oneTwo, Complex oneThree,
            Complex oneFour, Complex oneFive,
            Complex twoThree, Complex twoFour, Complex twoFive, Complex threeFour, Complex threeFive, Complex fourFive)
        {
            return DenseMatrix.OfArray(new[,]
            {
                {
                    oneTwo + oneThree + oneFour + oneFive, (-1)*oneTwo, (-1)*oneThree, (-1)*oneFour, (-1)*oneFive
                },
                {
                    (-1)*oneTwo, oneTwo + twoThree + twoFour + twoFive, (-1)*twoThree, (-1)*twoFour, (-1)*twoFive
                },
                {
                    (-1)*oneThree, (-1)*twoThree, oneThree + twoThree + threeFour + threeFive, (-1)*threeFour, (-1)*threeFive
                },
                {
                    (-1)*oneFour, (-1)*twoFour, (-1)*threeFour, oneFour + twoFour + threeFour + fourFive, (-1)*fourFive
                },
                {
                    (-1)*oneFive, (-1)*twoFive, (-1)*threeFive, (-1)*fourFive, oneFive + twoFive + threeFive + fourFive
                }
            });
        }

        protected static void CreateThreeNodeProblemWithGroundNode(out Matrix<Complex> admittances, out Vector<Complex> voltages, out Vector<Complex> powers,
            out double nominalVoltage)
        {
            admittances = CreateThreeNodeProblemAdmittanceMatrix(new Complex(1000, 500), new Complex(0, 0), new Complex(10, -60));

            voltages = new DenseVector(new []{new Complex(1.0, 0.12), new Complex(0.9, 0.1), new Complex(0, 0)});
            var currents = admittances.Multiply(voltages);
            powers = voltages.PointwiseMultiply(currents.Conjugate());
            nominalVoltage = 1;
        }

        protected static Matrix<Complex> CreateThreeNodeProblemAdmittanceMatrix(Complex oneTwo, Complex oneThree,
            Complex twoThree)
        {
            return DenseMatrix.OfArray(new[,]
            {
                {oneTwo + oneThree, (-1)*oneTwo, (-1)*oneThree},
                {(-1)*oneTwo, oneTwo + twoThree, (-1)*twoThree},
                {(-1)*oneThree, (-1)*twoThree, oneThree + twoThree}
            });
        }
    }
}
