﻿using LoadFlowCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class NewtonRaphsonMethodTest : LoadFlowCalculatorHighAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new NewtonRaphsonMethod(0.0001, 1000);
        }
    }
}