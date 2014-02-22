using System;
using System.Numerics;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public abstract class AnalyticContinuationTest
    {
        protected PowerSeries<double> _powerSeriesExponential;
        protected IAnalyticContinuation<double> _continuationExponential;

        protected abstract IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries);

        [TestInitialize]
        public void SetUp()
        {
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            _continuationExponential = CreateAnalyticContinuation(_powerSeriesExponential);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt2_ResultIsAsAccurateAsDirectPowerSeries()
        {
            const double x = 2.0;
            var correctValue = Math.Exp(x);
            var directValue = _powerSeriesExponential.Evaluate(x);
            var analyticContinuatedValue = _continuationExponential.Evaluate(x);
            var directError = Math.Abs(directValue - correctValue);
            var analyticContinuatedError = Math.Abs(analyticContinuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(analyticContinuatedValue));
            Assert.AreEqual(directValue, analyticContinuatedValue, 0.01);
            Assert.AreEqual(correctValue, analyticContinuatedValue, 0.01);
            Assert.AreEqual(directValue, analyticContinuatedValue, 0.01);
            Assert.IsTrue(directError > analyticContinuatedError*0.1);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt15_ResultIsAsAccurateAsDirectPowerSeries()
        {
            const double x = 1.5;
            var correctValue = Math.Exp(x);
            var directValue = _powerSeriesExponential.Evaluate(x);
            var analyticContinuatedValue = _continuationExponential.Evaluate(x);
            var directError = Math.Abs(directValue - correctValue);
            var analyticContinuatedError = Math.Abs(analyticContinuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(analyticContinuatedValue));
            Assert.AreEqual(correctValue, directValue, 0.01);
            Assert.AreEqual(correctValue, analyticContinuatedValue, 0.01);
            Assert.AreEqual(directValue, analyticContinuatedValue, 0.0001);
            Assert.IsTrue(directError > analyticContinuatedError*0.1);
        }

        [TestMethod]
        public void EvaluateAt1_Empty_SameAsEvaluateCalledWith1()
        {
            Assert.AreEqual(_continuationExponential.Evaluate(1), _continuationExponential.EvaluateAt1());
        }

        [TestMethod]
        public void Evaluate_ZeroOfLaguerrePolynom_CorrectResult()
        {
            var series = CreateLaguerreSeries(8);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.AreEqual(0.5857864375, result, 0.0001);
        }

        public static PowerSeriesDouble CreateLaguerreSeries(int n)
        {
            var sums = new double[n];
            sums[0] = 0;

            for (var i = 1; i < n; ++i)
                sums[i] = (sums[i - 1]*sums[i - 1] + 2)/4;

            var powerSeries = new PowerSeriesDouble(n);

            for (var i = 1; i < n; ++i)
                powerSeries[i] = sums[i] - sums[i - 1];

            return powerSeries;
        }
    }
}
