using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Numerics;

namespace LoadFlowCalculationTest
{
    public static class ComplexAssert
    {
        public static void AreEqual(Complex expected, Complex actual, double delta)
        {
            Assert.AreEqual(expected.Real, actual.Real, delta);
            Assert.AreEqual(expected.Imaginary, actual.Imaginary, delta);
        }

        public static void AreEqual(double expectedReal, double expectedImaginary, Complex actual, double delta)
        {
            Assert.AreEqual(expectedReal, actual.Real, delta);
            Assert.AreEqual(expectedImaginary, actual.Imaginary, delta);
        }
    }
}
