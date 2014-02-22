using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class ContinuedFractionTest
    {
        [TestMethod]
        public void Evaluate_ExponentialSeriesAt01_correctResult()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var continuedFraction = new ContinuedFraction<double>(powerSeries);
            const double x = 0.1;
            var correctValue = Math.Exp(x);

            Assert.AreEqual(correctValue, powerSeries.Evaluate(x), 0.0001);
            Assert.AreEqual(powerSeries.Evaluate(x), continuedFraction.Evaluate(x), 0.0001);
        }
    }
}
