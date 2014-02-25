using System;
using System.Numerics;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
