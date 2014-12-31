using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class CurrentIterationTest : NodeVoltageCalculatorTest
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
            return new CurrentIteration(0.00000001, 1000, true);
        }
    }
}
