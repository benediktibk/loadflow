using System;
using System.Numerics;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class PadeApproximantTest : AnalyticContinuationTest
    {
        protected override IAnalyticContinuation<double> CreateAnalyticContinuation(PowerSeries<double> powerSeries)
        {
            return new PadeApproximant<double>(3, 4, powerSeries, new CalculatorDouble());
        }
        protected override IAnalyticContinuation<Complex> CreateAnalyticContinuation(PowerSeries<Complex> powerSeries)
        {
            return new PadeApproximant<Complex>(24, 25, powerSeries, new CalculatorComplex());
        }

        [TestMethod]
        public void Constructor_ExponentialFunction_CoefficientsAreCorrect()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(2, 3, powerSeries, new CalculatorDouble());

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
            var padeApproximant = new PadeApproximant<double>(0, 2, powerSeries, new CalculatorDouble());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_2And0_ThrowsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(2, 0, powerSeries, new CalculatorDouble());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_4And5ButOnly9Coefficients_ThrowsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(9, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries, new CalculatorDouble());
        }

        [TestMethod]
        public void Constructor_4And5And10Coefficients_RunsThrough()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries, new CalculatorDouble());
        }

        [TestMethod]
        public void Evaluate_SinAt20_ResultIsMoreAccurateThanDirectPowerSeries()
        {
            const double x = 20;
            var correctValue = Math.Sin(x);
            var directValue = _powerSeriesSine.Evaluate(x);
            var continuatedValue = _continuationSine.Evaluate(x);
            var directError = Math.Abs(directValue - correctValue);
            var continuatedError = Math.Abs(continuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(directValue));
            Assert.IsFalse(Double.IsInfinity(directValue));
            Assert.IsFalse(Double.IsNaN(continuatedValue));
            Assert.IsFalse(Double.IsInfinity(continuatedValue));
            Assert.IsTrue(directError > continuatedError);
        }
    }
}
