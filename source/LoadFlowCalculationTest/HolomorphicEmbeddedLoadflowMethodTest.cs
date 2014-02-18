using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class HolomorphicEmbeddedLoadflowMethodTest :
        LoadFlowCalculatorHighAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new HolomorphicEmbeddedLoadFlowMethod(0.0001, 100);
        }
    }
}
