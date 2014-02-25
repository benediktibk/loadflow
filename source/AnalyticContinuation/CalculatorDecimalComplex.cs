using System;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public class CalculatorDecimalComplex : ICalculatorGeneric<DecimalComplex>
    {
        public DecimalComplex Add(DecimalComplex a, DecimalComplex b)
        {
            throw new NotImplementedException();
        }

        public DecimalComplex Subtract(DecimalComplex a, DecimalComplex b)
        {
            throw new NotImplementedException();
        }

        public DecimalComplex Multiply(DecimalComplex a, DecimalComplex b)
        {
            throw new NotImplementedException();
        }

        public DecimalComplex Divide(DecimalComplex a, DecimalComplex b)
        {
            throw new NotImplementedException();
        }

        public DecimalComplex Pow(DecimalComplex a, int exponent)
        {
            throw new NotImplementedException();
        }

        public DecimalComplex AssignFromDouble(double x)
        {
            throw new NotImplementedException();
        }

        public Matrix<DecimalComplex> CreateDenseMatrix(int rows, int columns)
        {
            throw new NotImplementedException();
        }

        public Vector<DecimalComplex> CreateDenseVector(int n)
        {
            throw new NotImplementedException();
        }

        public Vector<DecimalComplex> SolveEquationSystem(Matrix<DecimalComplex> matrix, Vector<DecimalComplex> vector)
        {
            throw new NotImplementedException();
        }

        public bool IsValidNumber(DecimalComplex x)
        {
            throw new NotImplementedException();
        }
    }
}
