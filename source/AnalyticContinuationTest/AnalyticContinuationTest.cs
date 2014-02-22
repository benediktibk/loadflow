using System;
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
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(8, new CalculatorDouble());
            _continuationExponential = new PadeApproximant<double>(3, 4, _powerSeriesExponential);
            _powerSeriesSine = PowerSeriesDouble.CreateSine(8, new CalculatorDouble());
            _continuationSine = new PadeApproximant<double>(3, 4, _powerSeriesSine);
        }

        [TestMethod]
        public void Evaluate_ExponentialAt2_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            var correctValue = Math.Exp(2);
            var directValue = _powerSeriesExponential.Evaluate(2);
            var analyticContinuatedValue = _continuationExponential.Evaluate(2);
            var directError = Math.Abs(directValue - correctValue);
            var analyticContinuatedError = Math.Abs(analyticContinuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(analyticContinuatedValue));
            Assert.IsTrue(directError > analyticContinuatedError);
        }

        [TestMethod]
        public void Evaluate_SinAt1_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            var correctValue = Math.Sin(1);
            var directValue = _powerSeriesSine.Evaluate(2);
            var analyticContinuatedValue = _continuationSine.Evaluate(2);
            var directError = Math.Abs(directValue - correctValue);
            var analyticContinuatedError = Math.Abs(analyticContinuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(analyticContinuatedValue));
            Assert.IsTrue(directError > analyticContinuatedError);
        }

        [TestMethod]
        public void EvaluateAt1_Empty_SameAsEvaluateCalledWith1()
        {
            Assert.AreEqual(_continuationExponential.Evaluate(1), _continuationExponential.EvaluateAt1());
        }
    }
}
