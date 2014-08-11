using System;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

namespace Database
{
    class CalculatorDirect : ICalculator
    {
        #region variables

        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        #endregion

        #region constructor

        public CalculatorDirect(INodeVoltageCalculator nodeVoltageCalculator)
        {
            if (nodeVoltageCalculator == null)
                throw new ArgumentNullException("nodeVoltageCalculator");

            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        #endregion

        #region ICalculator

        public bool Calculate(SymmetricPowerNet powerNet)
        {
            return powerNet.CalculateNodeVoltages(_nodeVoltageCalculator);
        }

        #endregion
    }
}
