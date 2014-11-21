using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNetComputable : IPowerNet
    {
        IList<NodeResult> CalculateNodeResults(out double relativePowerError);
        INodeVoltageCalculator NodeVoltageCalculator { get; }
    }
}