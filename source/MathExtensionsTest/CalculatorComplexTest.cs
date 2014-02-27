using System;
using System.Numerics;
using MathExtensions;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace MathExtensionsTest
{
    [TestClass]
    public class CalculatorComplexTest
    {
        private Complex _one;
        private Complex _two;
        private CalculatorComplex _calculator;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CalculatorComplex();
            _one = new Complex(1, -2);
            _two = new Complex(Math.PI, Math.E);
        }

        [TestMethod]
        public void Add_OneAndTwo_CorrectResult()
        {
            Assert.AreEqual(_one + _two, _calculator.Add(_one, _two));
        }

        [TestMethod]
        public void Subtract_OneAndTwo_CorrectResult()
        {
            Assert.AreEqual(_one - _two, _calculator.Subtract(_one, _two));
        }

        [TestMethod]
        public void Multiply_OneAndTwo_CorrectResult()
        {
            Assert.AreEqual(_one * _two, _calculator.Multiply(_one, _two));
        }

        [TestMethod]
        public void Divide_OneAndTwo_CorrectResult()
        {
            Assert.AreEqual(_one / _two, _calculator.Divide(_one, _two));
        }

        [TestMethod]
        public void AssignFromDouble_Pi_CorrectResult()
        {
            Assert.AreEqual(new Complex(Math.PI, 0), _calculator.AssignFromDouble(Math.PI));
        }

        [TestMethod]
        public void CreateDenseVector_5_CountIs5()
        {
            var vector = _calculator.CreateDenseVector(5);

            Assert.AreEqual(5, vector.Count);
        }

        [TestMethod]
        public void SolveEquationSystem_SolvableProblem_CorrectSolution()
        {
            var x = new DenseVector(new[]{ new Complex(1, 0), new Complex(2, -1)});
            var A = DenseMatrix.OfArray(new[,] { { new Complex(-2, 1), new Complex(2, 3) }, { new Complex(-1, 5), new Complex(0, 2)} });
            var b = A.Multiply(x);

            var solution = _calculator.SolveEquationSystem(A, b);

            Assert.AreEqual(x.Count, solution.Count);
            Assert.AreEqual(x.At(0).Real, solution.At(0).Real, 0.0001);
            Assert.AreEqual(x.At(0).Imaginary, solution.At(0).Imaginary, 0.0001);
            Assert.AreEqual(x.At(1).Real, solution.At(1).Real, 0.0001);
            Assert.AreEqual(x.At(1).Imaginary, solution.At(1).Imaginary, 0.0001);
        }

        [TestMethod]
        public void Pow_4And2_16()
        {
            ComplexAssert.AreEqual(new Complex(16, 0), _calculator.Pow(new Complex(4, 0), 2), 0.0001);
        }

        [TestMethod]
        public void IsValidNumber_4And1_true()
        {
            Assert.IsTrue(_calculator.IsValidNumber(new Complex(4, 1)));
        }

        [TestMethod]
        public void IsValidNumber_4AndInf_true()
        {
            Assert.IsFalse(_calculator.IsValidNumber(new Complex(4, Double.PositiveInfinity)));
        }

        [TestMethod]
        public void IsValidNumber_InfAnd2_true()
        {
            Assert.IsFalse(_calculator.IsValidNumber(new Complex(Double.PositiveInfinity, 2)));
        }

        [TestMethod]
        public void IsValidNumber_4AndNaN_true()
        {
            Assert.IsFalse(_calculator.IsValidNumber(new Complex(4, Double.NaN)));
        }

        [TestMethod]
        public void IsValidNumber_NaNAnd2_true()
        {
            Assert.IsFalse(_calculator.IsValidNumber(new Complex(Double.NaN, 2)));
        }
    }
}
