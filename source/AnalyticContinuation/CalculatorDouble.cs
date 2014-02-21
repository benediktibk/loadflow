using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public class CalculatorDouble : ICalculatorGeneric<double>
    {
        public double Add(double a, double b)
        {
            return a + b;
        }

        public double Subtract(double a, double b)
        {
            return a - b;
        }

        public double Multiply(double a, double b)
        {
            return a * b;
        }

        public double Divide(double a, double b)
        {
            return a / b;
        }

        public double AssignFromDouble(double x)
        {
            return x;
        }

        public Matrix<double> CreateDenseMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        public Vector<double> CreateDenseVector(int n)
        {
            return new DenseVector(n);
        }
    }
}
