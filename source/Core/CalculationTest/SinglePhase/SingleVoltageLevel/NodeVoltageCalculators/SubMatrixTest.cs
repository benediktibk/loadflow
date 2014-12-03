using System;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class SubMatrixTest
    {
        private DenseMatrix _matrix;
        private SubMatrix _subMatrix;

        [TestInitialize]
        public void SetUp()
        {
            _matrix = DenseMatrix.OfArray(new double[,]
            {
                {1, 2, 3, 4}, 
                {5, 6, 7, 8}, 
                {9, 10, 11, 12}
            });
            _subMatrix = new SubMatrix(_matrix, 0, 1, 2, 3);
        }

        [TestMethod]
        public void Get_IndicesInRange_CorrectValue()
        {
            Assert.AreEqual(6, _subMatrix[1, 0], 0.000001);
        }

        [TestMethod]
        public void Set_IndicesInRange_CorrectValueInMatrixSet()
        {
            _subMatrix[0, 1] = 100;

            Assert.AreEqual(100, _matrix[0, 2], 0.000001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Set_RowNotInRange_ExceptionThrown()
        {
            _subMatrix[2, 1] = 100;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Set_ColumnNotInRange_ExceptionThrown()
        {
            _subMatrix[0, 3] = 100;
        }
    }
}
