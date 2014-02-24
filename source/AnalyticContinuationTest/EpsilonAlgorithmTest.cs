using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class EpsilonAlgorithmTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new EpsilonAlgorithm<double>(powerSeries);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt10_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            const double x = 10;
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
