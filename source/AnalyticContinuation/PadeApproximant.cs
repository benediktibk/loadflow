using System;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public class PadeApproximant<T> where T : struct, IEquatable<T>, IFormattable
    {
        private PowerSeries<T> _p;
        private PowerSeries<T> _q;
        private readonly ICalculatorGeneric<T> _calculator; 

        public PadeApproximant(int L, int M, PowerSeries<T> powerSeries)
        {
            _calculator = powerSeries.Calculator;
            if (L < 1)
                throw new ArgumentOutOfRangeException("L", "the degree for the power series in the nominator must be at least 1");
            if (M < 1)
                throw new ArgumentOutOfRangeException("M", "the degree for the power series in the denominator must be at least 1");

            if (L + M + 2 > powerSeries.NumberOfCoefficients + 1)
                throw new ArgumentOutOfRangeException("L", "there are not enough source coefficients for this setup");

            _p = new PowerSeries<T>(L + 1, _calculator);
            _q = new PowerSeries<T>(M + 1, _calculator);

            CalculateCoefficientsForQ(powerSeries);
            CalculateCoefficientsForP(powerSeries);
        }

        public T Evaluate(T x)
        {
            return _calculator.Divide(_p.Evaluate(x), _q.Evaluate(x));
        }

        public T EvaluateAt1()
        {
            return _calculator.Divide(_p.EvaluateAt1(), _q.EvaluateAt1());
        }

        public T GetNominatorCoefficient(int m)
        {
            return _p[m];
        }

        public T GetDenominatorCoefficient(int n)
        {
            return _q[n];
        }

        private void CalculateCoefficientsForQ(PowerSeries<T> powerSeries)
        {
            Matrix<T> matrix = _calculator.CreateDenseMatrix(M, M);
            Vector<T> rightSide = _calculator.CreateDenseVector(M);
            var minusOne = _calculator.AssignFromDouble(-1);

            var tempArray = new T[M];
            for (var i = 0; i < M; ++i)
                tempArray[i] = _calculator.Multiply(powerSeries[L + i + 1], minusOne);

            rightSide.SetValues(tempArray);

            for (var column = 0; column < M; ++column)
            {
                for (var row = 0; row < M; ++row)
                    tempArray[row] = powerSeries[L + row - column];

                matrix.SetColumn(column, tempArray);
            }

            var q = _calculator.SolveEquationSystem(matrix, rightSide);
            _q[0] = _calculator.AssignFromDouble(1);

            for (var i = 0; i < q.Count; ++i)
                _q[i + 1] = q[i];
        }

        private void CalculateCoefficientsForP(PowerSeries<T> powerSeries)
        {
            for (var i = 0; i <= L; ++i)
            {
                var p = powerSeries[i];

                for (var j = 0; j < i; ++j)
                {
                    var a = powerSeries[j];
                    var q = _q[i - j];
                    p = _calculator.Add(p, _calculator.Multiply(a, q));
                }

                _p[i] = p;
            }
        }

        public int L
        {
            get { return _p.Degree; }
        }

        public int M
        {
            get { return _q.Degree; }
        }
    }
}
