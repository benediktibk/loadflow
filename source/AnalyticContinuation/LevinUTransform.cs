using System;
using MathExtensions;

namespace AnalyticContinuation
{
    public class LevinUTransform<T> : LevinTransform<T> where T : struct, IEquatable<T>, IFormattable
    {
        public LevinUTransform(PowerSeries<T> powerSeries, int b, int n) : base(powerSeries, b, n)
        { }

        protected override T EvaluateG(int i)
        {
            throw new NotImplementedException();
        }
    }
}
