using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public abstract class HolomorphicEmbeddedLoadFlowMethodTest : LoadFlowCalculatorTest
    {
        private readonly List<HolomorphicEmbeddedLoadFlowMethod> _highAccuracyCalculator = new List<HolomorphicEmbeddedLoadFlowMethod>();

        protected override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            var calculator = CreateHELMNodeVoltageCalculator();
            _highAccuracyCalculator.Add(calculator);
            return calculator;
        }

        protected abstract HolomorphicEmbeddedLoadFlowMethod CreateHELMNodeVoltageCalculator();

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
            var result = HolomorphicEmbeddedLoadFlowMethodTestNativeMethods.RunTests();
            Assert.IsTrue(result);
        }

        protected static void CalculateCorrectCoefficientsForTwoNodesWithImaginaryConnectionAndPVBusVersionTwo(out Complex a,
            out Complex b, out Complex c)
        {
            a = new Complex(1.05, 0);
            b = new Complex(-0.062673010380623, -0.0403690888119954);
            c = new Complex(0.0686026762176026, 0.0475978097324825);
        }
    }
}
