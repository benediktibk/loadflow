using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyticContinuation
{
    public class EpsilonAlgorithm<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator;
        private List<T> _epsilonPrevious;
        private List<T> _epsilonCurrent;
        private List<T> _epsilonNext;
 
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
            var n = _epsilonCurrent.Count();
            var reduced = new List<T>(n) {_epsilonCurrent[0]};

            for (var i = 1; i < n; ++i)
                if (!_epsilonCurrent[i].Equals(reduced[reduced.Count - 1]))
                    reduced.Add(_epsilonCurrent[i]);

            _epsilonCurrent = reduced;
        }

        private void InitializeNextEpsilon()
        {
            _epsilonNext = new List<T>(_epsilonCurrent.Count() - 1);
        }

        private T ExecuteEpsilonAlgorithm()
        {
            var n = _epsilonCurrent.Count();

            do
            {
                for (var j = 0; j <= _epsilonCurrent.Count - 2; ++j)
                {
                    var previousDifference = _calculator.Subtract(_epsilonCurrent[j + 1], _epsilonCurrent[j]);
                    var invers = _calculator.Divide(_calculator.AssignFromDouble(1), previousDifference);
                    _epsilonNext.Add(_calculator.Add(_epsilonPrevious[j + 1], invers));
                }

                _epsilonPrevious = _epsilonCurrent;
                _epsilonCurrent = _epsilonNext;
                _epsilonNext = new List<T>(_epsilonCurrent.Count - 1);
            } while (_epsilonCurrent.Count > 2);

            return n % 2 == 0
                ? _epsilonCurrent[_epsilonCurrent.Count - 1]
                : _epsilonPrevious[_epsilonPrevious.Count - 1];
        }

        private void InitializeCurrentEpsilon(T x)
        {
            _epsilonCurrent = new List<T>(_powerSeries.EvaluatePartialSums(x));
        }

        private void InitializeCurrentEpsilon()
        {
            _epsilonCurrent = new List<T>(_powerSeries.EvaluatePartialSumsAt1());
        }

        private void InitializePreviousEpsilon()
        {
            var n = _epsilonCurrent.Count() + 1;
            _epsilonPrevious = new List<T>(n);
            for (var i = 0; i < n; ++i)
                _epsilonPrevious.Add(_calculator.AssignFromDouble(0));
        }
    }
}
