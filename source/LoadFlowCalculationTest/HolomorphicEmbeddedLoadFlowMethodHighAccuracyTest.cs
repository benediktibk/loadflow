using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public abstract class HolomorphicEmbeddedLoadFlowMethodHighAccuracyTest : LoadFlowCalculatorTest
    {
        private readonly List<HolomorphicEmbeddedLoadFlowMethodHighAccuracy> _highAccuracyCalculator = new List<HolomorphicEmbeddedLoadFlowMethodHighAccuracy>();

        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            var calculator = CreateHighAccuracyLoadFlowCalculator();
            _highAccuracyCalculator.Add(calculator);
            return calculator;
        }

        protected abstract HolomorphicEmbeddedLoadFlowMethodHighAccuracy CreateHighAccuracyLoadFlowCalculator();

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

        [DllImport("LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool RunTests();
    }
}
