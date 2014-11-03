using System;
using System.Collections.Generic;
using Calculation;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

namespace Database
{
    class CalculatorDirect : ICalculator
    {
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public CalculatorDirect(INodeVoltageCalculator nodeVoltageCalculator)
        {
            if (nodeVoltageCalculator == null)
                throw new ArgumentNullException("nodeVoltageCalculator");

            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IReadOnlyDictionary<long, NodeResult> Calculate(SymmetricPowerNet powerNet)
        {
            powerNet.NodeVoltageCalculator = _nodeVoltageCalculator;
            return powerNet.CalculateNodeVoltages();
        }
    }
}
