using System;
using System.Collections.Generic;
using MathNet.Numerics.Signals;

namespace AnalyticContinuation
{
    public class AitkenProcess<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator;

        public AitkenProcess(PowerSeries<T> powerSeries)
        {
            if (powerSeries.NumberOfCoefficients < 3)
                throw new ArgumentOutOfRangeException("powerSeries",
                    "the power series must have at least three coefficients");
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            var partialSums = _powerSeries.EvaluatePartialSums(x);
            return Evaluate(partialSums);
        }

        public T EvaluateAt1()
        {
            var partialSums = _powerSeries.EvaluatePartialSumsAt1();
            return Evaluate(partialSums);
        }

        private T Evaluate(IEnumerable<T> s)
        {
            var previous = new List<T>(s);

            do
            {
                var n = previous.Count - 2;
                var next = new List<T>(n);

                for (var i = 0; i < n; ++i)
                {
                    var singleDifference = _calculator.Subtract(previous[i + 1], previous[i]);
                    var doubleDifference = _calculator.Add(previous[i + 2],
                        _calculator.Add(previous[i],
                            _calculator.Multiply(_calculator.AssignFromDouble(-2), previous[i + 1])));
                    var nominator = _calculator.Pow(singleDifference, 2);
                    var denominator = doubleDifference;
                    var modifier = _calculator.Divide(nominator, denominator);
                    next.Add(_calculator.Subtract(previous[i], modifier));
                }

                previous = next;
            } while (previous.Count >= 3);

            return previous[previous.Count - 1];
        }
    }
}
