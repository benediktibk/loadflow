using System;
using System.Runtime.InteropServices;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public abstract class HolomorphicEmbeddedLoadFlowMethodHighAccuracyTest : LoadFlowCalculatorTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return CreateHighAccuracyLoadFlowCalculator();
        }

        protected abstract HolomorphicEmbeddedLoadFlowMethodHighAccuracy CreateHighAccuracyLoadFlowCalculator();

        [TestMethod]
        public void CppUnitTests()
        {
            var result = RunTests();
            Assert.IsTrue(result);
        }

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RunTests();
    }
}
