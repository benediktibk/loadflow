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

            if (m + n + 2 > powerSeries.GetDegree() + 1)
                throw new ArgumentOutOfRangeException();

            _p = new PowerSeries<T>(m + 1, _calculator);
            _q = new PowerSeries<T>(n + 1, _calculator);

            CalculateCoefficientsForQ();
            CalculateCoefficientsForP();
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
            return _p.GetCoefficient(m);
        }

        public T GetDenominatorCoefficient(int n)
        {
            return _q.GetCoefficient(n);
        }

        private void CalculateCoefficientsForQ()
        {
            var L = _p.GetDegree();
            var M = _q.GetDegree();
            Matrix<T> matrix = _calculator.CreateDenseMatrix(M, M);
            Vector<T> rightSide = _calculator.CreateDenseVector(M);
            throw new NotImplementedException();
        }

        private void CalculateCoefficientsForP()
        {
            throw new NotImplementedException();
        }
    }
}
