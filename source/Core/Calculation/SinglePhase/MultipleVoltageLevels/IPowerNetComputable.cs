using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetComputable : IPowerNet
    {
        IReadOnlyDictionary<long, NodeResult> CalculateNodeResults(out double relativePowerError);
        void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling);
    }
}