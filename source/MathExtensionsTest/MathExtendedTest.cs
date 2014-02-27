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
    }
}
