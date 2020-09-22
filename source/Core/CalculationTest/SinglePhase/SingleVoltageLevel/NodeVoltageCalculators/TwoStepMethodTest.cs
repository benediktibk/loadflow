using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class TwoStepMethodTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionOnePqBus
        {
            get { return 0.0001; }
        }

        public override double PrecisionTwoPvBuses
        {
            get { return 0.0001; }
        }

        public override double PrecisionOnePqAndOnePv
        {
            get { return 0.0001; }
        }

        public override double PrecisionThreePqBuses
        {
            get { return 0.0001; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new TwoStepMethod(new HolomorphicEmbeddedLoadFlowMethod(0.000001, 50, 64, true),
                new CurrentIteration(0.000001, 1000, true));
        }
    }
}
