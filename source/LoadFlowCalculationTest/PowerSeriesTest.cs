using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class PowerSeriesTest
    {
        private PowerSeries _exponential;
        private PowerSeries _sin;

        [TestInitialize]
        public void SetUp()
        {
            _exponential = PowerSeries.CreateExponential(50);
            _sin = PowerSeries.CreateSin(10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_0_exceptionThrown()
        {
            var function = new PowerSeries(0);
        }

        [TestMethod]
        public void Evaluatae_exponentialFunctionAt1_e()
        {
            Assert.AreEqual(Math.E, _exponential.Evaluate(1).Magnitude, 0.0001);
        }

        [TestMethod]
        public void Evaluate_exponentialFunctionAt2_ESquare()
        {
            Assert.AreEqual(Math.Pow(Math.E, 2), _exponential.Evaluate(2).Magnitude, 0.0001);
        }

        [TestMethod]
        public void Evaluate_exponentialFunctionAt0_1()
        {
            Assert.AreEqual(1, _exponential.Evaluate(0).Magnitude, 0.0001);
        }

        [TestMethod]
        public void EvaluateAt1_empty_sameResultAsEvaluatedDirectAt1()
        {
            ComplexAssert.AreEqual(_exponential.Evaluate(1), _exponential.EvaluateAt1(), 0.000001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt0_0()
        {
            Assert.AreEqual(0, _sin.Evaluate(0).Magnitude, 0.0001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt3Half_correctResult()
        {
            Assert.AreEqual(Math.Sin(1.5), _sin.Evaluate(1.5).Magnitude, 0.0001);
        }

        [TestMethod]
        public void Evaluate_sinFunctionAt1_correctResult()
        {
            Assert.AreEqual(Math.Sin(1), _sin.Evaluate(1).Magnitude, 0.0001);
        }
    }
}
