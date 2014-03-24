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

        [TestMethod]
        public void CalculateNodeVoltagesAndPowers_TwoNodesWithImaginaryConnectionAndPVBusVersionTwo_CorrectCoefficients()
        {
            var nodes = CreateTestTwoNodesWithImaginaryConnectionWithPVBusVersionTwo();
            var calculator = CreateHELMLoadFlowCalculator();

            calculator.CalculateNodeVoltagesAndPowers(_admittances, _nominalVoltage, nodes, out _voltageCollapse);

            var admittance = new Complex(0, 50);
            var vSlack = new Complex(1.05, 0);
            const double vMagnitude = 1.02;
            const double vMagnitudeSquare = vMagnitude * vMagnitude;
            var a = vSlack;
            var b = a / vMagnitudeSquare * (vSlack * a + vMagnitudeSquare + 2 / admittance);
            var c = b / vMagnitudeSquare * (2 / admittance + vMagnitudeSquare - 2 * a * vSlack);
            var firstCoefficient = calculator.GetCoefficients(0)[0];
            var secondCoefficient = calculator.GetCoefficients(1)[0];
            var thirdCoefficient = calculator.GetCoefficients(2)[0];
            ComplexAssert.AreEqual(a, firstCoefficient, 0.00001);
            ComplexAssert.AreEqual(b, secondCoefficient, 0.00001);
            ComplexAssert.AreEqual(c, thirdCoefficient, 0.00001);
        }

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RunTests();
    }
}
