using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class PadeApproximantTest
    {
        private PowerSeries<double> _powerSeriesExponential;
        private PadeApproximant<double> _padeApproximantExponential;

        [TestInitialize]
        public void SetUp()
        {
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(5, new CalculatorDouble());
            _padeApproximantExponential = new PadeApproximant<double>(2, 2, _powerSeriesExponential);
        }

        [TestMethod]
        public void Evaluate_exponentialAt2_resultIsMoreAccurateThanDirectPowerSeries()
        {
            var correctValue = Math.Exp(2);
            var directValue = _powerSeriesExponential.Evaluate(2);
            var analyticContinuatedValue = _padeApproximantExponential.Evaluate(2);
            var directError = Math.Abs(directValue - correctValue);
            var analyticContinuatedError = Math.Abs(analyticContinuatedValue - correctValue);

            Assert.IsFalse(Double.IsNaN(analyticContinuatedValue));
            Assert.IsTrue(directError > analyticContinuatedError);
        }

        [TestMethod]
        public void EvaluateAt1_empty_sameAsEvaluateCalledWith1()
        {
            Assert.AreEqual<double>(_padeApproximantExponential.Evaluate(1), _padeApproximantExponential.EvaluateAt1());
        }

        [TestMethod]
        public void Constructor_exponentialFunction_coefficientsAreCorrect()
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
        public void Constructor_0And2_throwsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(0, 2, powerSeries);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_2And0_throwsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(2, 0, powerSeries);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_4And5ButOnly11Coefficients_throwsException()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(11, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries);
        }

        [TestMethod]
        public void Constructor_4And5And12Coefficients_runsThrough()
        {
            var powerSeries = PowerSeriesDouble.CreateExponential(12, new CalculatorDouble());
            var padeApproximant = new PadeApproximant<double>(4, 5, powerSeries);
        }
    }
}
