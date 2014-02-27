using System;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class LevinTransformTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new LevinTransform<double>(powerSeries);
        }

        protected override IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries)
        {
            return new LevinTransform<Complex>(powerSeries);
        }
    }
}
