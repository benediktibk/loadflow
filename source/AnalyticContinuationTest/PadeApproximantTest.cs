using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class PadeApproximantTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new PadeApproximant<double>(3, 4, powerSeries);
        }

        [TestMethod]
        public void Constructor_ExponentialFunction_CoefficientsAreCorrect()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(2, 3, powerSeries);

            Assert.AreEqual(1, padeApproximant.GetNominatorCoefficient(0), 0.0001);
            Assert.AreEqual(24.0/60, padeApproximant.GetNominatorCoefficient(1), 0.0001);
            Assert.AreEqual(3.0/60, padeApproximant.GetNominatorCoefficient(2), 0.0001);
            Assert.AreEqual(1, padeApproximant.GetDenominatorCoefficient(0), 0.0001);
            Assert.AreEqual(-36.0/60, padeApproximant.GetDenominatorCoefficient(1), 0.0001);
            Assert.AreEqual(9.0/60, padeApproximant.GetDenominatorCoefficient(2), 0.0001);
            Assert.AreEqual(-1.0/60, padeApproximant.GetDenominatorCoefficient(3), 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_0And2_ThrowsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(0, 2, powerSeries);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_2And0_ThrowsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(2, 0, powerSeries);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_4And5ButOnly9Coefficients_ThrowsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(9, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries);
        }

        [TestMethod]
        public void Constructor_4And5And10Coefficients_RunsThrough()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries);
        }
    }
}
