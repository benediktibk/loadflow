using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoadFlowCalculation;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class PowerSeriesTest
    {
        PowerSeries _exponential;

        [TestInitialize]
        public void SetUp()
        {
            _exponential = PowerSeries.CreateExponential(50);
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
    }
}
