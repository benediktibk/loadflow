using System;
using System.Collections.Generic;
using MathExtensions;

namespace AnalyticContinuation
{
    public class LevinUTransform<T> : LevinTransform<T> where T : struct, IEquatable<T>, IFormattable
    {
        public LevinUTransform(PowerSeries<T> powerSeries, int b, int n) : base(powerSeries, b, n)
        { }

        protected override T EvaluateG(int i, IList<T> coefficients)
        {
            var modifier = i + B;
            var modifierConverted = Calculator.AssignFromDouble(modifier);
            var coefficient = coefficients[i];
            return Calculator.Multiply(modifierConverted, coefficient);
        }
    }
}
