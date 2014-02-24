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

        [TestMethod]
        public void Evaluate_ZeroOfLaguerrePolynom_ResultIsCorrectForEpsilonAlgorithm()
        {
            var series = CreateLaguerreSeries(6);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.AreEqual(0.58578573, result, 0.00000001);
        }
    }
}
