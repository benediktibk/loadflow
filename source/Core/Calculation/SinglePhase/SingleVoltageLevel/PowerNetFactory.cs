using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNetFactory : IPowerNetFactory
    {
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public PowerNetFactory(INodeVoltageCalculator nodeVoltageCalculator)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IPowerNetComputable Create(IAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyList<Complex> constantCurrents)
        {
            return new PowerNetComputable(_nodeVoltageCalculator, admittances, nominalVoltage, constantCurrents);
        }
    }
}
