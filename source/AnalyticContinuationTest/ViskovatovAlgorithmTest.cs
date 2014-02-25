using System;
using System.Numerics;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class ViskovatovAlgorithmTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new ViskovatovAlgorithm<double>(powerSeries);
        }

        protected override IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries)
        {
            return new ViskovatovAlgorithm<Complex>(powerSeries);
        }

        [TestMethod]
        public void Evaluate_Ln2SeriesWithOnly4Coefficients_CorrectResult()
        {
            var series = CreateLn2Series(4);
            var continuation = CreateAnalyticContinuation(series);

            var result = continuation.Evaluate(1);

            Assert.IsFalse(Double.IsNaN(result));
            Assert.IsFalse(Double.IsInfinity(result));
            Assert.AreEqual(Math.Log(2), result, 0.01);
        }

        [TestMethod]
        public void Evaluate_VoltageSeriesFromHELM_CorrectResultWithLowAccuracy()
        {
            ComplexAssert.AreEqual(1, -0.1, _continuationVoltage.EvaluateAt1(), 0.1);
        }
    }
}
