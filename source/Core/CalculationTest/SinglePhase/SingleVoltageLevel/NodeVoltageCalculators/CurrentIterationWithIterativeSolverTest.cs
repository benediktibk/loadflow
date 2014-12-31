using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class CurrentIterationWithIterativeSolverTest : NodeVoltageCalculatorTest
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
            return new CurrentIteration(0.00000001, 1000, true);
        }
    }
}
