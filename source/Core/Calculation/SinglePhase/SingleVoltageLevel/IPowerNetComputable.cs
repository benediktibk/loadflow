using System.Collections.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNetComputable : IPowerNet
    {
        IList<NodeResult> CalculateNodeResults();
    }
}