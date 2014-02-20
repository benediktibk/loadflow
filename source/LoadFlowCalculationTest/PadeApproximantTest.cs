using System;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class PadeApproximantTest
    {
        private PowerSeries<double> _powerSeriesExponential;
        private PadeApproximantDouble _padeApproximantExponential;

        [TestInitialize]
        public void SetUp()
        {
            _powerSeriesExponential = PowerSeriesDouble.CreateExponential(4, new CalculatorDouble());
            _padeApproximantExponential = new PadeApproximantDouble(4, 4, _powerSeriesExponential);
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
    }
}
