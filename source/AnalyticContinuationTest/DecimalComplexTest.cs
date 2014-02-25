using AnalyticContinuation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticContinuationTest
{
    [TestClass]
    public class DecimalComplexTest
    {
        [TestMethod]
        public void Constructor_empty_RealIs0()
        {
            var value = new DecimalComplex();

            Assert.AreEqual(0, value.Real);
        }

        [TestMethod]
        public void Constructor_4And5_RealIs4()
        {
            var value = new DecimalComplex(4, 5);

            Assert.AreEqual(4, value.Real);
        }

        [TestMethod]
        public void Constructor_4And5_ImaginaryIs5()
        {
            var value = new DecimalComplex(4, 5);

            Assert.AreEqual(5, value.Imaginary);
        }

        [TestMethod]
        public void Constructor_empty_ImaginaryIs0()
        {
            var value = new DecimalComplex();

            Assert.AreEqual(0, value.Imaginary);
        }

        [TestMethod]
        public void Equals_RealAndImaginaryEqual_true()
        {
            var one = new DecimalComplex();
            var two = new DecimalComplex();
            one.Real = 2;
            one.Imaginary = -1;
            two.Real = 2;
            two.Imaginary = -1;

            Assert.AreEqual(one, two);
        }

        [TestMethod]
        public void Equals_RealDifferent_false()
        {
            var one = new DecimalComplex();
            var two = new DecimalComplex();
            one.Real = 1;
            one.Imaginary = -1;
            two.Real = 2;
            two.Imaginary = -1;

            Assert.AreNotEqual(one, two);
        }

        [TestMethod]
        public void Equals_ImaginaryDifferent_false()
        {
            var one = new DecimalComplex();
            var two = new DecimalComplex();
            one.Real = 2;
            one.Imaginary = 10;
            two.Real = 2;
            two.Imaginary = -1;

            Assert.AreNotEqual(one, two);
        }
    }
}
