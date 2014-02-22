using System;

namespace AnalyticContinuation
{
    public interface IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        T Evaluate(T x);
        T EvaluateAt1();
    }
}