using System;
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
            var partialSums = _powerSeries.EvaluatePartialSums(x);
            return EvaluateInternal(partialSums);
        }

        public T EvaluateAt1()
        {
            var partialSums = _powerSeries.EvaluatePartialSumsAt1();
            return EvaluateInternal(partialSums);
        }

        private T EvaluateInternal(T[] partialSums)
        {
            throw new NotImplementedException();
        }

        protected abstract T EvaluateG(int i);
    }
}
