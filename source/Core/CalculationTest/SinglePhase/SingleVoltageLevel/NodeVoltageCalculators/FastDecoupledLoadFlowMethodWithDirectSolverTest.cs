using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodWithDirectSolverTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 10; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 10; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.0000001, 1000, false);
        }
    }
}
