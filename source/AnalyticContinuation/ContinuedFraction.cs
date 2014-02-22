using System;

namespace AnalyticContinuation
{
    public class ContinuedFraction<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator; 

        public ContinuedFraction(PowerSeries<T> powerSeries)
        {
            if (powerSeries.NumberOfCoefficients < 3)
                throw new ArgumentOutOfRangeException("powerSeries",
                    "the power series must have at least 3 coefficients");
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            T result = _calculator.AssignFromDouble(0);
            var n = _powerSeries.NumberOfCoefficients;
            var coefficients = new T[n];
            coefficients[0] = _powerSeries[0];

            for (var i = 1; i < n; ++i)
                coefficients[i] = _calculator.Multiply(_powerSeries[i], _calculator.Pow(x, i));

            T nominator;
            T denominator;
            var negative = _calculator.AssignFromDouble(-1);

            for (var i = _powerSeries.NumberOfCoefficients - 1; i >= 3; --i)
            {
                nominator = _calculator.Multiply(coefficients[i - 2], coefficients[i]);
                var partialDenominator = _calculator.Add(coefficients[i - 1], coefficients[i]);
                denominator = _calculator.Add(partialDenominator, result);
                result = _calculator.Multiply(negative, _calculator.Divide(nominator, denominator));
            }

            nominator = _calculator.Multiply(negative, coefficients[2]);
            denominator = _calculator.Add(coefficients[1], _calculator.Add(coefficients[2], result));
            result = _calculator.Divide(nominator, denominator);
            nominator = coefficients[1];
            denominator = _calculator.Add(_calculator.AssignFromDouble(1), result);
            result = _calculator.Divide(nominator, denominator);
            result = _calculator.Add(coefficients[0], result);

            return result;
        }
    }
}
