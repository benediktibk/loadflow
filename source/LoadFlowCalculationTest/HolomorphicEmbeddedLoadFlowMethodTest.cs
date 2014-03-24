using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public abstract class HolomorphicEmbeddedLoadFlowMethodTest : LoadFlowCalculatorTest
    {
        private readonly List<HolomorphicEmbeddedLoadFlowMethod> _highAccuracyCalculator = new List<HolomorphicEmbeddedLoadFlowMethod>();

        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            var calculator = CreateHELMLoadFlowCalculator();
            _highAccuracyCalculator.Add(calculator);
            return calculator;
        }

        protected abstract HolomorphicEmbeddedLoadFlowMethod CreateHELMLoadFlowCalculator();

        [TestCleanup]
        public void CleanUp()
        {
            foreach (var calculator in _highAccuracyCalculator)
                calculator.Dispose();

            _highAccuracyCalculator.Clear();
        }

        [TestMethod]
        public void CppUnitTests()
        {
            var result = RunTests();
            Assert.IsTrue(result);
        }

        protected static void CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out Complex a,
            out Complex b, out Complex c)
        {
            var admittance = new Complex(0, 50);
            var vSlack = new Complex(1.05, 0);
            const double vMagnitude = 1.02;
            const double vMagnitudeSquare = vMagnitude*vMagnitude;
            a = vSlack;
            b = a/vMagnitudeSquare*(vSlack*a + vMagnitudeSquare + 2/admittance);
            c = b/vMagnitudeSquare*(2/admittance + vMagnitudeSquare - 2*a*vSlack);
        }

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RunTests();
    }
}
