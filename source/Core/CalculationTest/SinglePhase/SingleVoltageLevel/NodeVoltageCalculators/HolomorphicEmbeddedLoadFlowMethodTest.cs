using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodTest
    {
        [TestMethod]
        public void CppUnitTests()
        {
            var result = HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTests();
            Assert.IsTrue(result);
        }

        public static void CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out Complex a,
            out Complex b, out Complex c)
        {
            a = new Complex(1.05, 0);
            b = new Complex(-0.062673010380623, -0.0403690888119954);
            c = new Complex(0.0686026762176026, 0.0475978097324825);
        }
    }
}
