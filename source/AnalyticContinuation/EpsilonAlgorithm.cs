using System;
using System.Linq;

namespace AnalyticContinuation
{
    public class EpsilonAlgorithm<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator; 
        private T[] _epsilonPrevious;
        private T[] _epsilonCurrent;
        private T[] _epsilonNext;
 
        public EpsilonAlgorithm(PowerSeries<T> powerSeries)
        {
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
            _epsilonPrevious = new T[powerSeries.NumberOfCoefficients + 1];
            _epsilonCurrent = new T[powerSeries.NumberOfCoefficients];
            _epsilonNext = new T[powerSeries.NumberOfCoefficients - 1];
        }

        public T Evaluate(T x)
        {
            for (var i = 0; i < _epsilonPrevious.Count(); ++i)
                _epsilonPrevious[i] = _calculator.AssignFromDouble(0);

            var sum = _calculator.AssignFromDouble(0);
            for (var i = 0; i < _epsilonCurrent.Count(); ++i)
            {
                var xPotency = _calculator.Pow(x, i);
                var summand = _calculator.Multiply(_powerSeries[i], xPotency);
                sum = _calculator.Add(sum, summand);
                _epsilonCurrent[i] = sum;
            }

            for (var i = 1; i <= _powerSeries.Degree; ++i)
            {
                for (var j = 0; j <= _powerSeries.Degree - i; ++j)
                {
                    var previousDifference = _calculator.Subtract(_epsilonCurrent[j + 1], _epsilonCurrent[j]);
                    var invers = _calculator.Divide(_calculator.AssignFromDouble(1), previousDifference);
                    _epsilonNext[j] = _calculator.Add(_epsilonPrevious[j + 1], invers);
                }

                var temp = _epsilonPrevious;
                _epsilonPrevious = _epsilonCurrent;
                _epsilonCurrent = _epsilonNext;
                _epsilonNext = temp;
            }

            return _epsilonCurrent[0];
        }

        public T EvaluateAt1()
        {
            throw new NotImplementedException();
        }
    }
}
