using System;

namespace AnalyticContinuation
{
    public class EpsilonAlgorithm<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
 
        public EpsilonAlgorithm(PowerSeries<T> powerSeries)
        {
            _powerSeries = powerSeries;
        }

        public T Evaluate(T x)
        {
            throw new NotImplementedException();
        }

        public T EvaluateAt1()
        {
            throw new NotImplementedException();
        }
    }
}
