using System;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public interface ICalculatorGeneric<T> where T : struct, IEquatable<T>, IFormattable
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
        T AssignFromDouble(double x);
        Matrix<T> CreateDenseMatrix(int rows, int columns);
        Vector<T> CreateDenseVector(int n);
        Vector<T> SolveEquationSystem(Matrix<T> matrix, Vector<T> vector);
    }
}
