using System;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class CalculatorDecimalComplexTest
    {
        private CalculatorDecimalComplex _calculator;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CalculatorDecimalComplex();
        }

        [TestMethod]
        public void Add_4And5And6And1_10And6()
        {
            var one = new DecimalComplex(4, 5);
            var two = new DecimalComplex(6, 1);

            var result = _calculator.Add(one, two);

            Assert.AreEqual(new DecimalComplex(10, 6), result);
        }

        [TestMethod]
        public void Subtract_4And5And6And1_Minus2And4()
        {
            var one = new DecimalComplex(4, 5);
            var two = new DecimalComplex(6, 1);

            var result = _calculator.Subtract(one, two);

            Assert.AreEqual(new DecimalComplex(-2, 4), result);
        }

        [TestMethod]
        public void Multiply_4And5And6And1_CorrectResult()
        {
            var one = new DecimalComplex(4, 5);
            var two = new DecimalComplex(6, 1);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(29.0 / 35, (double)result.Real, 0.00001);
            Assert.AreEqual(26.0 / 35, (double)result.Imaginary, 0.00001);
        }

        [TestMethod]
        public void AssignFromDouble_10_10And0()
        {
            Assert.AreEqual(new DecimalComplex(10, 0), _calculator.AssignFromDouble(10));
        }

        [TestMethod]
        public void IsValidNumber_4And5_True()
        {
            Assert.IsTrue(_calculator.IsValidNumber(new DecimalComplex(4, 5)));
        }
    }
}
