using System;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class SparseMatrixStorageTest
    {
        private SparseMatrixStorage _matrix;

        [TestInitialize]
        public void SetUp()
        {
            _matrix = new SparseMatrixStorage(5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeDimension_ThrowsException()
        {
            new SparseMatrixStorage(-2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_ZeroDimension_ThrowsException()
        {
            new SparseMatrixStorage(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_NegativeRow_ThrowsException()
        {
            _matrix[-2, 2] = 10.2;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_NegativeColumn_ThrowsException()
        {
            _matrix[1, -2] = 10.2;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_RowAsBigAsDimension_ThrowsException()
        {
            _matrix[5, 2] = 10.2;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Indexer_ColumnAsBigAsDimension_ThrowsException()
        {
            _matrix[1, 5] = 10.2;
        }

        [TestMethod]
        public void ToMatrix_CompleteRowSet_CorrectValues()
        {
            _matrix[2, 0] = 10;
            _matrix[2, 1] = 20;
            _matrix[2, 2] = 30;
            _matrix[2, 3] = 40;
            _matrix[2, 4] = 50;

            var result = _matrix.ToMatrix();

            Assert.AreEqual(5, CalculateNonZeroCount(result));
            Assert.AreEqual(10, result[2, 0], 1e-10);
            Assert.AreEqual(20, result[2, 1], 1e-10);
            Assert.AreEqual(30, result[2, 2], 1e-10);
            Assert.AreEqual(40, result[2, 3], 1e-10);
            Assert.AreEqual(50, result[2, 4], 1e-10);
        }

        [TestMethod]
        public void ToMatrix_CompleteRowInverseSet_CorrectValues()
        {
            _matrix[2, 4] = 50;
            _matrix[2, 3] = 40;
            _matrix[2, 2] = 30;
            _matrix[2, 1] = 20;
            _matrix[2, 0] = 10;

            var result = _matrix.ToMatrix();

            Assert.AreEqual(5, CalculateNonZeroCount(result));
            Assert.AreEqual(10, result[2, 0], 1e-10);
            Assert.AreEqual(20, result[2, 1], 1e-10);
            Assert.AreEqual(30, result[2, 2], 1e-10);
            Assert.AreEqual(40, result[2, 3], 1e-10);
            Assert.AreEqual(50, result[2, 4], 1e-10);
        }

        [TestMethod]
        public void ToMatrix_CompleteRowRandomSet_CorrectValues()
        {
            _matrix[2, 3] = 40;
            _matrix[2, 1] = 20;
            _matrix[2, 4] = 50;
            _matrix[2, 0] = 10;
            _matrix[2, 2] = 30;

            var result = _matrix.ToMatrix();

            Assert.AreEqual(5, CalculateNonZeroCount(result));
            Assert.AreEqual(10, result[2, 0], 1e-10);
            Assert.AreEqual(20, result[2, 1], 1e-10);
            Assert.AreEqual(30, result[2, 2], 1e-10);
            Assert.AreEqual(40, result[2, 3], 1e-10);
            Assert.AreEqual(50, result[2, 4], 1e-10);
        }

        [TestMethod]
        public void ToMatrix_CompleteRowRandomAdded_CorrectValues()
        {
            _matrix[2, 3] = 20;
            _matrix[2, 1] = 10;
            _matrix[2, 4] = 25;
            _matrix[2, 0] = 5;
            _matrix[2, 2] = 15;
            _matrix[2, 3] += 20;
            _matrix[2, 1] += 10;
            _matrix[2, 4] += 25;
            _matrix[2, 0] += 5;
            _matrix[2, 2] += 15;

            var result = _matrix.ToMatrix();

            Assert.AreEqual(5, CalculateNonZeroCount(result));
            Assert.AreEqual(10, result[2, 0], 1e-10);
            Assert.AreEqual(20, result[2, 1], 1e-10);
            Assert.AreEqual(30, result[2, 2], 1e-10);
            Assert.AreEqual(40, result[2, 3], 1e-10);
            Assert.AreEqual(50, result[2, 4], 1e-10);
        }

        [TestMethod]
        public void Reset_Empty_EmptyMatrix()
        {
            _matrix[2, 3] = 40;
            _matrix[2, 1] = 20;
            _matrix[2, 4] = 50;
            _matrix[2, 0] = 10;
            _matrix[2, 2] = 30;

            _matrix.Reset();

            var result = _matrix.ToMatrix();
            Assert.AreEqual(0, CalculateNonZeroCount(result));
        }

        private static int CalculateNonZeroCount(Matrix<double> matrix)
        {
            return matrix.Enumerate(Zeros.AllowSkip).Count();
        }
    }
}
