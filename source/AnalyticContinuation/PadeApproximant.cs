using System;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace AnalyticContinuation
{
    public class PadeApproximant<T> where T : struct, IEquatable<T>, IFormattable
    {
        private PowerSeries<T> _p;
        private PowerSeries<T> _q;
        private readonly ICalculatorGeneric<T> _calculator; 

        public PadeApproximant(int m, int n, PowerSeries<T> powerSeries)
        {
            _calculator = powerSeries.GetCalculator();
            if (m < 1 || n < 1)
                throw new ArgumentOutOfRangeException();

            if (m + n + 2 > powerSeries.GetNumberOfCoefficients() + 1)
                throw new ArgumentOutOfRangeException();

            _p = new PowerSeries<T>(m + 1, _calculator);
            _q = new PowerSeries<T>(n + 1, _calculator);

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
            var L = _p.GetNumberOfCoefficients() - 1;
            var M = _q.GetNumberOfCoefficients() - 1;
            Matrix<T> matrix = _calculator.CreateDenseMatrix(M, M);
            Vector<T> rightSide = _calculator.CreateDenseVector(M);
            var tempArray = new T[M + 1];
            tempArray[0] = _calculator.AssignFromDouble(1);
            for (var i = 1; i <= M; ++i)
                tempArray[i] = powerSeries[L + i + 1];

            rightSide.SetValues(tempArray);

            for (var column = 0; column < M; ++column)
            {
                for (var row = 0; row < M; ++row)
                    tempArray[row] = powerSeries[L + row];

                matrix.SetColumn(column, tempArray);
            }

            var p = _calculator.SolveEquationSystem(matrix, rightSide);
            _p.SetCoefficients(p);
        }

        private void CalculateCoefficientsForP(PowerSeries<T> powerSeries)
        {
            var L = _p.GetNumberOfCoefficients() - 1;
            var M = _q.GetNumberOfCoefficients() - 1;

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
    }
}
