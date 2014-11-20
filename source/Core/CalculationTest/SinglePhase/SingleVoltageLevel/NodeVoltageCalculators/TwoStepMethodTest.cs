using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class TwoStepMethodTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 0.0001; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(0.000001, 50, 64),
                new CurrentIteration(0.000001, 1000));
        }
    }
}
