using System.Numerics;
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
    }
}
