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
            throw new NotImplementedException();
        }

        protected override IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries)
        {
            throw new NotImplementedException();
        }
    }
}
