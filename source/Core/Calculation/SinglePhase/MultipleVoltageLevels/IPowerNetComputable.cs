using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetComputable : IPowerNet
    {
        IReadOnlyDictionary<int, NodeResult> CalculateNodeResults(out double relativePowerError);
        void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling);
        double TotalProgress { get; }
    }
}