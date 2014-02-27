using System;
using System.Collections.Generic;
using MathExtensions;

namespace AnalyticContinuation
{
    public abstract class LevinTransform<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        protected readonly ICalculatorGeneric<T> Calculator;
        protected readonly int B;
        protected readonly int N;

        protected LevinTransform(PowerSeries<T> powerSeries, int b, int n)
        {
            _powerSeries = powerSeries;
            Calculator = powerSeries.Calculator;
            B = b;
            N = n;
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
            var summandsReduced = new List<T>(summands.Count);

            foreach (var summand in summands)
                if (!summand.Equals(new T()))
                    summandsReduced.Add(summand);

            var partialSums = new T[summandsReduced.Count];
            partialSums[0] = summandsReduced[0];

            for (var i = 1; i < summandsReduced.Count; ++i)
                partialSums[i] = Calculator.Add(partialSums[i - 1], summandsReduced[i]);

            var nominator = new T();
            var denominator = new T();
            var k = summandsReduced.Count - N;

            for (var j = 0; j <= k; ++j)
            {
                var sign = j%2 == 0 ? 1 : -1;
                var binomialCoefficient = MathExtended.BinomialCoefficient(k, j);
                var power = Math.Pow(N + j + B, k - 1);
                var modifier = sign*binomialCoefficient*power;
                var modifierConverted = Calculator.AssignFromDouble(modifier);
                var g = EvaluateG(N + j, summandsReduced);
                var coefficientNominator = Calculator.Divide(summandsReduced[N + j], g);
                var coefficientDenominator = Calculator.Divide(Calculator.AssignFromDouble(1), g);
                nominator = Calculator.Add(nominator, Calculator.Multiply(modifierConverted, coefficientNominator));
                denominator = Calculator.Add(denominator, Calculator.Multiply(modifierConverted, coefficientDenominator));
            }

            return Calculator.Divide(nominator, denominator);
        }

        protected abstract T EvaluateG(int i, IList<T> coefficients);
    }
}
