using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public class CalculatorComplex : ICalculatorGeneric<Complex>
    {
        public Complex Add(Complex a, Complex b)
        {
            return a + b;
        }

        public Complex Subtract(Complex a, Complex b)
        {
            return a - b;
        }

        public Complex Multiply(Complex a, Complex b)
        {
            return a * b;
        }

        public Complex Divide(Complex a, Complex b)
        {
            return a / b;
        }

        public Complex Pow(Complex a, int exponent)
        {
            return a.Power(exponent);
        }

        public Complex AssignFromDouble(double x)
        {
            return new Complex(x, 0);
        }

        public Matrix<Complex> CreateDenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Vector<Complex> CreateDenseVector(int n)
        {
            return new DenseVector(n);
        }

        public Vector<Complex> SolveEquationSystem(Matrix<Complex> matrix, Vector<Complex> vector)
        {
            var matrixCasted = (Matrix) matrix;
            var factorization = matrixCasted.QR();
            return factorization.Solve(vector);
        }

        public bool IsValidNumber(Complex x)
        {
            return !x.IsInfinity() && !x.IsNaN();
        }
    }
}
