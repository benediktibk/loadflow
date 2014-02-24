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
        protected PowerSeries<double> _powerSeriesSine; 
        protected IAnalyticContinuation<double> _continuationExponential;
        protected IAnalyticContinuation<double> _continuationSine; 

        protected abstract IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries);

        [TestInitialize]
        public void SetUp()
        {
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            _powerSeriesSine = PowerSeriesDouble.CreateSine(10, new CalculatorDouble());
            _continuationExponential = CreateAnalyticContinuation(_powerSeriesExponential);
            _continuationSine = CreateAnalyticContinuation(_powerSeriesSine);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt2_CorrectResult()
        {
            const double x = 2.0;

            Assert.AreEqual(Math.Exp(x), _continuationExponential.Evaluate(x), 0.01);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt15_CorrectResult()
        {
            const double x = 1.5;

            Assert.AreEqual(Math.Exp(x), _continuationExponential.Evaluate(x), 0.001);
        }

        [TestMethod]
        public void Evaluate_SineAt1_CorrectResult()
        {
            const double x = 1;

            Assert.AreEqual(Math.Sin(x), _continuationSine.Evaluate(x), 0.0001);
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
