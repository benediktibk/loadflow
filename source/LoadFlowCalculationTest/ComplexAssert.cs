using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace LoadFlowCalculationTest
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
    }
}
