using System;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodTest : LoadFlowCalculatorSmallAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.001, 10000);
        }
    }
}
