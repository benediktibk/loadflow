using System;
using AnalyticContinuation;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class CalculatorDoubleTest
    {
        private CalculatorDouble _calculator;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CalculatorDouble();
        }

        [TestMethod]
        public void Add_piAndE_correctResult()
        {
            Assert.AreEqual(Math.E + Math.PI, _calculator.Add(Math.E, Math.PI));
        }

        [TestMethod]
        public void Subtract_piAndE_correctResult()
        {
            Assert.AreEqual(Math.E - Math.PI, _calculator.Subtract(Math.E, Math.PI));
        }

        [TestMethod]
        public void Multiply_piAndE_correctResult()
        {
            Assert.AreEqual(Math.E * Math.PI, _calculator.Multiply(Math.E, Math.PI));
        }

        [TestMethod]
        public void Divide_piAndE_correctResult()
        {
            Assert.AreEqual(Math.E / Math.PI, _calculator.Divide(Math.E, Math.PI));
        }

        [TestMethod]
        public void AssignFromDouble_pi_correctResult()
        {
            Assert.AreEqual(Math.PI, _calculator.AssignFromDouble(Math.PI));
        }

        [TestMethod]
        public void CreateDenseMatrix_3And4_3RowsAnd4Columns()
        {
            var matrix = _calculator.CreateDenseMatrix(3, 4);

            Assert.AreEqual(3, matrix.RowCount);
            Assert.AreEqual(4, matrix.ColumnCount);
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
            var x = new DenseVector(new double[] {1, 2});
            var A = DenseMatrix.OfArray(new double[,] {{2, 3}, {-1, 5}});
            var b = A.Multiply(x);

            var solution = _calculator.SolveEquationSystem(A, b);

            Assert.AreEqual(x.Count, solution.Count);
            Assert.AreEqual(x.At(0), solution.At(0), 0.0001);
            Assert.AreEqual(x.At(1), solution.At(1), 0.0001);
        }
    }
}
