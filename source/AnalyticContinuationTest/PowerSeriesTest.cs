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
            _sin = PowerSeriesDouble.CreateSine(10, new CalculatorDouble());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_0_ExceptionThrown()
        {
            var function = new PowerSeriesDouble(0);
        }

        [TestMethod]
        public void Evaluatae_ExponentialFunctionAt1_E()
        {
            Assert.AreEqual(Math.E, _exponential.Evaluate(1), 0.0001);
        }

        [TestMethod]
        public void Evaluate_ExponentialFunctionAt2_ESquare()
        {
            Assert.AreEqual(Math.Pow(Math.E, 2), _exponential.Evaluate(2), 0.01);
        }

        [TestMethod]
        public void Evaluate_ExponentialFunctionAt0_1()
        {
            Assert.AreEqual(1, _exponential.Evaluate(0), 0.0001);
        }

        [TestMethod]
        public void EvaluateAt1_Empty_SameResultAsEvaluatedDirectAt1()
        {
            Assert.AreEqual(_exponential.Evaluate(1), _exponential.EvaluateAt1(), 0.000001);
        }

        [TestMethod]
        public void Evaluate_SinFunctionAt0_0()
        {
            Assert.AreEqual(0, _sin.Evaluate(0), 0.0001);
        }

        [TestMethod]
        public void Evaluate_SinFunctionAt3Half_CorrectResult()
        {
            Assert.AreEqual(Math.Sin(1.5), _sin.Evaluate(1.5), 0.0001);
        }

        [TestMethod]
        public void Evaluate_SinFunctionAt1_CorrectResult()
        {
            Assert.AreEqual(Math.Sin(1), _sin.Evaluate(1), 0.0001);
        }

        [TestMethod]
        public void GetNumberOfCoefficients_PowerSeriesWith10Coefficients_10()
        {
            Assert.AreEqual(10, _exponential.GetNumberOfCoefficients());
        }

        [TestMethod]
        public void GetDegree_PowerSeriesWith10Coefficients_9()
        {
            Assert.AreEqual(9, _exponential.GetDegree());
        }
    }
}
