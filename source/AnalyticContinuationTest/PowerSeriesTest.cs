using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class PowerSeriesTest
    {
        private PowerSeries<double> _exponential;
        private PowerSeries<double> _sin;
        
        [TestInitialize]
        public void SetUp()
        {
            _exponential = PowerSeriesDouble.CreateExponential(10, new CalculatorDouble());
            _sin = PowerSeriesDouble.CreateSin(10, new CalculatorDouble());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_0_exceptionThrown()
        {
            var function = new PowerSeriesDouble(0);
        }

        [TestMethod]
        public void Evaluatae_exponentialFunctionAt1_e()
        {
            Assert.AreEqual(Math.E, _exponential.Evaluate(1), 0.0001);
        }

        [TestMethod]
        public void Evaluate_exponentialFunctionAt2_ESquare()
        {
            Assert.AreEqual(Math.Pow(Math.E, 2), _exponential.Evaluate(2), 0.01);
        }

        [TestMethod]
        public void Evaluate_exponentialFunctionAt0_1()
        {
            Assert.AreEqual(1, _exponential.Evaluate(0), 0.0001);
        }

        [TestMethod]
        public void EvaluateAt1_empty_sameResultAsEvaluatedDirectAt1()
        {
            Assert.AreEqual(_exponential.Evaluate(1), _exponential.EvaluateAt1(), 0.000001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt0_0()
        {
            Assert.AreEqual(0, _sin.Evaluate(0), 0.0001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt3Half_correctResult()
        {
            Assert.AreEqual(Math.Sin(1.5), _sin.Evaluate(1.5), 0.0001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt1_correctResult()
        {
            Assert.AreEqual(Math.Sin(1), _sin.Evaluate(1), 0.0001);
        }
    }
}
