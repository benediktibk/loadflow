using System;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace UnitTestHelper
{
    public static class ComplexAssert
    {
        public static void AreEqual(Complex expected, Complex actual, double delta)
        {
            Assert.IsFalse(Double.IsNaN(actual.Real));
            Assert.IsFalse(Double.IsNaN(actual.Imaginary));
            Assert.IsFalse(Double.IsInfinity(actual.Real));
            Assert.IsFalse(Double.IsInfinity(actual.Imaginary));
            Assert.AreEqual(expected.Real, actual.Real, delta);
            Assert.AreEqual(expected.Imaginary, actual.Imaginary, delta);
        }

        public static void AreEqual(double expectedReal, double expectedImaginary, Complex actual, double delta)
        {
            AreEqual(new Complex(expectedReal, expectedImaginary), actual, delta);
        }

        public static void AreAllEqual(Vector<Complex> expected, Vector<Complex> actual, double delta)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; ++i)
                AreEqual(expected[i], actual[i], delta);
        }
    }
}
