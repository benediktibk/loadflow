using System;
using System.Numerics;
using AnalyticContinuation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
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
        public void Add_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one + _two, _calculator.Add(_one, _two));
        }

        [TestMethod]
        public void Subtract_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one - _two, _calculator.Subtract(_one, _two));
        }

        [TestMethod]
        public void Multiply_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one * _two, _calculator.Multiply(_one, _two));
        }

        [TestMethod]
        public void Divide_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one / _two, _calculator.Divide(_one, _two));
        }

        [TestMethod]
        public void AssignFromDouble_pi_correctResult()
        {
            Assert.AreEqual(new Complex(Math.PI, 0), _calculator.AssignFromDouble(Math.PI));
        }

        [TestMethod]
        public void CreateDenseVector_5_countIs5()
        {
            var vector = _calculator.CreateDenseVector(5);

            Assert.AreEqual(5, vector.Count);
        }

        [TestMethod]
        public void SolveEquationSystem_solvableProblem_correctSolution()
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
    }
}
