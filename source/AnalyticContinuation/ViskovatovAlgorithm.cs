using System;

namespace AnalyticContinuation
{
    public class ViskovatovAlgorithm<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
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
