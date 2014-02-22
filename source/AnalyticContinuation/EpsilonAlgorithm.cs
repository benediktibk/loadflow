using System;
using System.Collections.Generic;
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
            if (powerSeries.Degree < 1)
                throw new ArgumentOutOfRangeException("powerSeries",
                    "there must be at least two coefficients in the power series");
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            InitializeCurrentEpsilon(x);
            ReduceRedundancyInCurrentEpsilons();
            InitializePreviousEpsilon();
            InitializeNextEpsilon();
            return ExecuteEpsilonAlgorithm();
        }

        public T EvaluateAt1()
        {
            InitializeCurrentEpsilon();
            ReduceRedundancyInCurrentEpsilons();
            InitializePreviousEpsilon();
            InitializeNextEpsilon();
            return ExecuteEpsilonAlgorithm();
        }

        private void ReduceRedundancyInCurrentEpsilons()
        {
            var reduced = new List<T>(_epsilonCurrent.Count());
            reduced.Add(_epsilonCurrent[0]);

            for (var i = 1; i < _epsilonCurrent.Count(); ++i)
                if (!_epsilonCurrent[i].Equals(reduced[reduced.Count - 1]))
                    reduced.Add(_epsilonCurrent[i]);

            _epsilonCurrent = reduced.ToArray();
        }

        private void InitializeNextEpsilon()
        {
            _epsilonNext = new T[_epsilonCurrent.Count()];
        }

        private T ExecuteEpsilonAlgorithm()
        {
            var n = _epsilonCurrent.Count();

            for (var i = 1; i <= n - 1; ++i)
            {
                for (var j = 0; j <= n - 2; ++j)
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

            return n % 2 == 1
                ? _epsilonCurrent[0]
                : _calculator.Divide(_calculator.Add(_epsilonPrevious[0], _epsilonPrevious[1]),
                    _calculator.AssignFromDouble(2));
        }

        private void InitializeCurrentEpsilon(T x)
        {
            _epsilonCurrent = new T[_powerSeries.NumberOfCoefficients];
            var sum = _calculator.AssignFromDouble(0);
            for (var i = 0; i < _epsilonCurrent.Count(); ++i)
            {
                var xPotency = _calculator.Pow(x, i);
                var summand = _calculator.Multiply(_powerSeries[i], xPotency);
                sum = _calculator.Add(sum, summand);
                _epsilonCurrent[i] = sum;
            }
        }

        private void InitializeCurrentEpsilon()
        {
            _epsilonCurrent = new T[_powerSeries.NumberOfCoefficients];
            var sum = _calculator.AssignFromDouble(0);
            for (var i = 0; i < _epsilonCurrent.Count(); ++i)
            {
                sum = _calculator.Add(sum, _powerSeries[i]);
                _epsilonCurrent[i] = sum;
            }
        }

        private void InitializePreviousEpsilon()
        {
            _epsilonPrevious = new T[_epsilonCurrent.Count() + 1];
            for (var i = 0; i < _epsilonPrevious.Count(); ++i)
                _epsilonPrevious[i] = _calculator.AssignFromDouble(0);
        }
    }
}
