using System;
using System.Collections.Generic;
using MathExtensions;

namespace AnalyticContinuation
{
    public class ViskovatovAlgorithm<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator; 

        public ViskovatovAlgorithm(PowerSeries<T> powerSeries)
        {
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            var summands = CalculateSummands(x);
            return Evaluate(summands);
        }

        private List<T> CalculateSummands(T x)
        {
            var summands = new List<T>(_powerSeries.NumberOfCoefficients);

            for (var i = 0; i < _powerSeries.NumberOfCoefficients; ++i)
            {
                var xPotency = _calculator.Pow(x, i);
                var coefficient = _powerSeries[i];
                var summand = _calculator.Multiply(coefficient, xPotency);

                if (!summand.Equals(_calculator.AssignFromDouble(0)))
                    summands.Add(summand);
            }
            return summands;
        }

        public T EvaluateAt1()
        {
            var summands = CalculateSummands();
            return Evaluate(summands);
        }

        private List<T> CalculateSummands()
        {
            var summands = new List<T>(_powerSeries.NumberOfCoefficients);

            for (var i = 0; i < _powerSeries.NumberOfCoefficients; ++i)
                if (!_powerSeries[i].Equals(_calculator.AssignFromDouble(0)))
                    summands.Add(_powerSeries[i]);
            return summands;
        }

        private T Evaluate(IList<T> summands)
        {
            var c = InitializeCoefficients(summands);
            var n = CalculateCoefficients(ref c);
            return CalculateContinuedFraction(c, n);
        }

        private T CalculateContinuedFraction(IReadOnlyList<T[]> c, int n)
        {
            var result = _calculator.AssignFromDouble(0);
            for (var i = n - 1; i >= 1; --i)
            {
                var denominator = _calculator.Add(c[i - 1][0], result);
                var nominator = c[i][0];
                result = _calculator.Divide(nominator, denominator);
            }
            return result;
        }

        private int CalculateCoefficients(ref List<T[]> c)
        {
            var n = c.Count;

            for (var k = 2; k < n; ++k)
                for (var j = 0; j < n - k; ++j)
                {
                    var firstProduct = _calculator.Multiply(c[k - 1][0], c[k - 2][j + 1]);
                    var secondProduct = _calculator.Multiply(c[k - 2][0], c[k - 1][j + 1]);
                    var coefficient = _calculator.Subtract(firstProduct, secondProduct);

                    if (!_calculator.IsValidNumber(coefficient))
                        return k;

                    c[k][j] = coefficient;
                }

            return n;
        }

        private List<T[]> InitializeCoefficients(IList<T> summands)
        {
            var n = summands.Count + 1;
            var c = new List<T[]>(n);

            for (var i = 0; i < n; ++i)
                c.Add(new T[n - i]);

            c[0][0] = _calculator.AssignFromDouble(1);
            for (var i = 0; i < summands.Count; ++i)
                c[1][i] = summands[i];
            return c;
        }
    }
}
