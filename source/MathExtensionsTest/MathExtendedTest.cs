using System;
using MathExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
