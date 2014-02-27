using System;
using System.Numerics;
using AnalyticContinuation;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class EpsilonAlgorithmTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new EpsilonAlgorithm<double>(powerSeries);
        }

        protected override IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries)
        {
            return new EpsilonAlgorithm<Complex>(powerSeries);
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

        [TestMethod]
        public void Evaluate_ZeroOfLaguerrePolynom_ResultIsCorrectForEpsilonAlgorithm()
        {
            var series = CreateLaguerreSeries(6);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.AreEqual(0.58578573, result, 0.00000001);
        }

        [TestMethod]
        public void Evaluate_VoltageSeriesFromHELM_CorrectResultWithLowAccuracy()
        {
            ComplexAssert.AreEqual(1, -0.1, _continuationVoltage.EvaluateAt1(), 0.1);
        }

        [TestMethod]
        public void Evaluate_VoltageSeriesFromHELMInDecimalComplex_CorrectResultWithLowAccuracy()
        {
            var powerSeries = CreateHelmVoltageSeriesDecimalComplex(32);
            var continuation = new EpsilonAlgorithm<DecimalComplex>(powerSeries);

            var result = continuation.EvaluateAt1();

            Assert.AreEqual(1, (double)result.Real, 0.1);
            Assert.AreEqual(-0.1, (double)result.Imaginary, 0.1);
        }
    }
}
