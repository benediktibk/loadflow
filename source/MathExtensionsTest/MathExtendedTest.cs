using System;
using System.Numerics;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace MathExtensionsTest
{
    [TestClass]
    public class MathExtendedTest
    {
        [TestMethod]
        public void Factorial_0_1()
        {
            Assert.AreEqual(1, MathExtended.Factorial(0));
        }

        [TestMethod]
        public void Factorial_5_125()
        {
            Assert.AreEqual(120, MathExtended.Factorial(5));
        }

        [TestMethod]
        public void Factorial_1_1()
        {
            Assert.AreEqual(1, MathExtended.Factorial(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Factorial_Minus1_ExceptionThrown()
        {
            MathExtended.Factorial(-1);
        }

        [TestMethod]
        public void BinomialCoefficient_9And4_126()
        {
            Assert.AreEqual(126, MathExtended.BinomialCoefficient(9, 4));
        }

        [TestMethod]
        public void BinomialCoefficient_5And3_10()
        {
            Assert.AreEqual(10, MathExtended.BinomialCoefficient(5, 3));
        }

        [TestMethod]
        public void BinomialCoefficient_5And2_10()
        {
            Assert.AreEqual(10, MathExtended.BinomialCoefficient(5, 2));
        }

        [TestMethod]
        public void BinomialCoefficient_9And9_1()
        {
            Assert.AreEqual(1, MathExtended.BinomialCoefficient(9, 9));
        }

        [TestMethod]
        public void BinomialCoefficient_9And0_1()
        {
            Assert.AreEqual(1, MathExtended.BinomialCoefficient(9, 0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BinomialCoefficient_9And10_ExceptionThrown()
        {
            MathExtended.BinomialCoefficient(9, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void BinomialCoefficient_9AndMinus1_ExceptionThrown()
        {
            MathExtended.BinomialCoefficient(9, -1);
        }

        [TestMethod]
        public void ComplexFromPolarCoordinates_Minus1And0_CorrectResult()
        {
            var result = Complex.FromPolarCoordinates(-1, 0);

            ComplexAssert.AreEqual(-1, 0, result, 0.00001);
        }

        [TestMethod]
        public void ComplexFromPolarCoordinates_NegativeMagnitude_CorrectResult()
        {
            var source = new Complex(0.25, -0.5);

            var result = Complex.FromPolarCoordinates(source.Magnitude*(-1), source.Phase + Math.PI);

            ComplexAssert.AreEqual(source, result, 0.00001);
        }
    }
}
