using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class HolomorphicEmbeddedLoadFlowMethodMultiPrecisionWithIterativeSolverTest : NodeVoltageCalculatorTest
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
            return new HolomorphicEmbeddedLoadFlowMethod(0.0000001, 70, 100, true);
        }
    }
}
