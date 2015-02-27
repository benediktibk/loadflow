using System;
using System.IO;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class BiCGSTABTest
    {
        private IIterativeSolver<Complex> _solver;
        private Iterator<Complex> _iterator;
        private IPreconditioner<Complex> _preconditioner;
        
        [TestInitialize]
        public void SetUp()
        {
            _solver = new BiCgStab();
            _iterator = new Iterator<Complex>();
            _preconditioner = new DiagonalPreconditioner();
        }

        [TestMethod]
        public void Solve_AdmittanceMatrixSmall_SmallError()
        {
            const int dimension = 2;
            var A = LoadMatrix("testdata\\matrix_small.csv", dimension);
            var b = LoadVector("testdata\\vector_small.csv", dimension);
            var x = new DenseVector(dimension);

            _solver.Solve(A, b, x, _iterator, _preconditioner);

            var error = CalculateError(A, x, b);
            Assert.AreEqual(0, error, 1e-5);
        }

        [TestMethod]
        public void Solve_AdmittanceMatrixOneVersionTwo_SmallError()
        {
            const int dimension = 15025;
            var A = LoadMatrix("testdata\\matrix.csv", dimension);
            var x = LoadVector("testdata\\vector.csv", dimension);
            var b = A*x;
            x = new DenseVector(dimension);

            _solver.Solve(A, b, x, _iterator, _preconditioner);

            var error = CalculateError(A, x, b);
            Assert.AreEqual(0, error, 1e-2);
        }

        private static double CalculateError(Matrix<Complex> A, Vector<Complex> x, Vector<Complex> b)
        {
            var bNorm = b.L2Norm();
            var residual = A*x - b;
            var residualNorm = residual.L2Norm();
            return residualNorm/bNorm;
        }

        private static SparseMatrix LoadMatrix(string fileName, int dimension)
        {
            var matrix = new SparseMatrix(dimension);
            var lines = File.ReadAllLines(fileName);

            foreach (var line in lines)
            {
                var cells = line.Split(';');
                var row = Int32.Parse(cells[0]);
                var column = Int32.Parse(cells[1]);
                var value = ParseComplex(cells[2]);
                matrix.At(row, column, value);
            }

            return matrix;
        }

        private static DenseVector LoadVector(string fileName, int dimension)
        {
            var vector = new DenseVector(dimension);
            var lines = File.ReadAllLines(fileName);

            if (lines.Count() != dimension)
                throw new ArgumentException("dimension");

            for (var i = 0; i < dimension; ++i)
            {
                var value = ParseComplex(lines[i]);
                vector.At(i, value);
            }

            return vector;
        }

        private static Complex ParseComplex(string cell)
        {
            var valueCells = cell.Split(',');
            var realString = valueCells[0].Substring(1);
            var real = Double.Parse(realString.Replace('.', ','));
            var imagString = valueCells[1].Substring(0, valueCells[1].Count() - 1);
            var imag = Double.Parse(imagString.Replace('.', ','));
            return new Complex(real, imag);
        }
    }
}
