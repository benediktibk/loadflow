using System;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace MathExtensions
{
    public interface ICalculatorMatrixOperationsGeneric<T> : ICalculatorGeneric<T> where T : struct, IEquatable<T>, IFormattable
    {
        Matrix<T> CreateDenseMatrix(int rows, int columns);
        Vector<T> CreateDenseVector(int n);
        Vector<T> SolveEquationSystem(Matrix<T> matrix, Vector<T> vector);
    }
}
