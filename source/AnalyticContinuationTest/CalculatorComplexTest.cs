using System;
using System.Numerics;
using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class CalculatorComplexTest
    {
        private Complex _one;
        private Complex _two;
        private CalculatorComplex _calculator;

        [TestInitialize]
        public void SetUp()
        {
            _calculator = new CalculatorComplex();
            _one = new Complex(1, -2);
            _two = new Complex(Math.PI, Math.E);
        }

        [TestMethod]
        public void Add_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one + _two, _calculator.Add(_one, _two));
        }

        [TestMethod]
        public void Subtract_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one - _two, _calculator.Subtract(_one, _two));
        }

        [TestMethod]
        public void Multiply_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one * _two, _calculator.Multiply(_one, _two));
        }

        [TestMethod]
        public void Divide_oneAndTwo_correctResult()
        {
            Assert.AreEqual(_one / _two, _calculator.Divide(_one, _two));
        }

        [TestMethod]
        public void AssignFromDouble_pi_correctResult()
        {
            Assert.AreEqual(new Complex(Math.PI, 0), _calculator.AssignFromDouble(Math.PI));
        }

        [TestMethod]
        public void CreateDenseVector_5_countIs5()
        {
            var vector = _calculator.CreateDenseVector(5);

            Assert.AreEqual(5, vector.Count);
        }
    }
}
