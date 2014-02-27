using System;
using System.Collections.Generic;
using MathExtensions;

namespace AnalyticContinuation
{
    /// <summary>
    /// Levin t-Transformation with n = 0 and b = 1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LevinTransform<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator;

        public LevinTransform(PowerSeries<T> powerSeries)
        {
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            var summands = _powerSeries.EvaluateSummands(x);
            return EvaluateInternal(summands);
        }

        public T EvaluateAt1()
        {
            var summands = _powerSeries.EvaluateSummandsAt1();
            return EvaluateInternal(summands);
        }

        private T EvaluateInternal(IList<T> summands)
        {
            var summandsReduced = ReduceSummands(summands);
            var partialSums = CalculatePartialSums(summandsReduced);

            var nominator = new T();
            var denominator = new T();
            var k = summandsReduced.Count - 2;

            for (var j = 0; j <= k; ++j)
            {
                var sign = j%2 == 0 ? 1 : -1;
                var binomialCoefficient = MathExtended.BinomialCoefficient(k, j);
                var power = Math.Pow(j + 1, k - 1)/Math.Pow(k + 1, k - 1);
                var modifier = sign*binomialCoefficient*power;
                var modifierConverted = _calculator.AssignFromDouble(modifier);
                var partialSum = partialSums[j];
                var summand = summandsReduced[j + 1];
                var nominatorPart = _calculator.Multiply(modifierConverted, _calculator.Divide(partialSum, summand));
                var denominatorPart = _calculator.Divide(modifierConverted, summand);
                nominator = _calculator.Add(nominator, nominatorPart);
                denominator = _calculator.Add(denominator, denominatorPart);
            }

            return _calculator.Divide(nominator, denominator);
        }

        private T[] CalculatePartialSums(IReadOnlyList<T> summandsReduced)
        {
            var partialSums = new T[summandsReduced.Count];
            partialSums[0] = summandsReduced[0];

            for (var i = 1; i < summandsReduced.Count; ++i)
                partialSums[i] = _calculator.Add(partialSums[i - 1], summandsReduced[i]);
            return partialSums;
        }

        private static List<T> ReduceSummands(ICollection<T> summands)
        {
            var summandsReduced = new List<T>(summands.Count);

            foreach (var summand in summands)
                if (!summand.Equals(new T()))
                    summandsReduced.Add(summand);

            return summandsReduced;
        }
    }
}
