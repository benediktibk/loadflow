using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExtensionsTest
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
        public void Multiply_4And5With6And1_CorrectResult()
        {
            var one = new DecimalComplex(4, 5);
            var two = new DecimalComplex(6, 1);

            var result = _calculator.Multiply(one, two);

            Assert.AreEqual(19, result.Real);
            Assert.AreEqual(34, result.Imaginary);
        }

        [TestMethod]
        public void Divide_4And5By6And1_CorrectResult()
        {
            var one = new DecimalComplex(4, 5);
            var two = new DecimalComplex(6, 1);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(0.783783783, (double)result.Real, 0.00001);
            Assert.AreEqual(0.702702702, (double)result.Imaginary, 0.00001);
        }

        [TestMethod]
        public void Divide_1And0ByMinus1216AndMinus1312_CorrectResult()
        {
            var one = new DecimalComplex(1, 0);
            var two = new DecimalComplex(-1216, -1312);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(-0.00038m, result.Real);
            Assert.AreEqual(0.00041m, result.Imaginary);
        }

        [TestMethod]
        public void Divide_1And0By10And0_CorrectResult()
        {
            var one = new DecimalComplex(1, 0);
            var two = new DecimalComplex(10, 0);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(0.1m, result.Real);
            Assert.AreEqual(0, result.Imaginary);
        }

        [TestMethod]
        public void Divide_1And0ByMinus10And0_CorrectResult()
        {
            var one = new DecimalComplex(1, 0);
            var two = new DecimalComplex(-10, 0);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(-0.1m, result.Real);
            Assert.AreEqual(0, result.Imaginary);
        }

        [TestMethod]
        public void Divide_1And0By0And10_CorrectResult()
        {
            var one = new DecimalComplex(1, 0);
            var two = new DecimalComplex(0, 10);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(0, result.Real);
            Assert.AreEqual(-0.1m, result.Imaginary);
        }

        [TestMethod]
        public void Divide_0And1By10And0_CorrectResult()
        {
            var one = new DecimalComplex(0, 1);
            var two = new DecimalComplex(10, 0);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(0, result.Real);
            Assert.AreEqual(0.1m, result.Imaginary);
        }

        [TestMethod]
        public void Divide_0And1By0And10_CorrectResult()
        {
            var one = new DecimalComplex(0, 1);
            var two = new DecimalComplex(0, 10);

            var result = _calculator.Divide(one, two);

            Assert.AreEqual(0.1m, result.Real);
            Assert.AreEqual(0, result.Imaginary);
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

        [TestMethod]
        public void Pow_4AndMinus2To3_CorrectResult()
        {
            var x = new DecimalComplex(4, -2);

            var result = _calculator.Pow(x, 3);

            Assert.AreEqual(16, result.Real);
            Assert.AreEqual(-88, result.Imaginary);
        }

        [TestMethod]
        public void Pow_4AndMinus2ToMinus5_CorrectResult()
        {
            var x = new DecimalComplex(4, -2);

            var result = _calculator.Pow(x, -5);

            Assert.AreEqual(-0.00038m, result.Real);
            Assert.AreEqual(0.00041m, result.Imaginary);
        }

        [TestMethod]
        public void Pow_4AndMinus2To5_CorrectResult()
        {
            var x = new DecimalComplex(4, -2);

            var result = _calculator.Pow(x, 5);

            Assert.AreEqual(-1216, result.Real);
            Assert.AreEqual(-1312, result.Imaginary);
        }

        [TestMethod]
        public void Pow_0To0_1()
        {
            var x = new DecimalComplex(0, 0);

            var result = _calculator.Pow(x, 0);

            Assert.AreEqual(1, result.Real);
            Assert.AreEqual(0, result.Imaginary);
        }
    }
}
