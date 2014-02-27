using System;
using System.Linq;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExtensionsTest
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
            Assert.AreEqual(10, _exponential.NumberOfCoefficients);
        }

        [TestMethod]
        public void GetDegree_PowerSeriesWith10Coefficients_9()
        {
            Assert.AreEqual(9, _exponential.Degree);
        }

        [TestMethod]
        public void EvaluatePartialSums_SeveralCoefficientsSet_CorrectResult()
        {
            var series = new PowerSeriesDouble(4);
            series[0] = 2;
            series[1] = 3;
            series[2] = -1;
            series[3] = 0.5;

            var result = series.EvaluatePartialSums(0.7);

            Assert.AreEqual(4, result.Count());
            Assert.AreEqual(2, result[0], 0.00001);
            Assert.AreEqual(4.1, result[1], 0.00001);
            Assert.AreEqual(3.61, result[2], 0.00001);
            Assert.AreEqual(3.7815, result[3], 0.00001);
        }

        [TestMethod]
        public void EvaluatePartialSumsAt1_Exponential_SameResultAsEvaluateParitalSums()
        {
            var result = _exponential.EvaluatePartialSumsAt1();

            var resultShouldBe = _exponential.EvaluatePartialSums(1);
            Assert.AreEqual(resultShouldBe.Count(), result.Count());
            for (var i = 0; i < result.Count(); ++i)
                Assert.AreEqual(resultShouldBe[i], result[i], 0.00001);
        }
    }
}
