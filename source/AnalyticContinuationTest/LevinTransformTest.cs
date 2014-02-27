using System;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

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

        [TestMethod]
        public void Evaluate_ExponentialAt2_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            const double x = 2;
            var correctValue = Math.Exp(x);
            var directValue = _powerSeriesExponential.Evaluate(x);
            var continuatedValue = _continuationExponential.Evaluate(x);
            var directError = Math.Abs(directValue - correctValue);
            var continuatedError = Math.Abs(continuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(directValue));
            Assert.IsFalse(Double.IsInfinity(directValue));
            Assert.IsFalse(Double.IsNaN(continuatedValue));
            Assert.IsFalse(Double.IsInfinity(continuatedValue));
            Assert.IsTrue(directError > continuatedError);
        }

        [TestMethod]
        public void Evaluate_ExponentialAtMinus4_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            const double x = -4;
            var correctValue = Math.Exp(x);
            var directValue = _powerSeriesExponential.Evaluate(x);
            var continuatedValue = _continuationExponential.Evaluate(x);
            var directError = Math.Abs(directValue - correctValue);
            var continuatedError = Math.Abs(continuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(directValue));
            Assert.IsFalse(Double.IsInfinity(directValue));
            Assert.IsFalse(Double.IsNaN(continuatedValue));
            Assert.IsFalse(Double.IsInfinity(continuatedValue));
            Assert.IsTrue(directError > continuatedError);
        }
    }
}
