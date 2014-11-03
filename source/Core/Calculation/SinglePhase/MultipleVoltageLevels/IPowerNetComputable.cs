using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetComputable : IPowerNet
    {
        IReadOnlyDictionary<long, NodeResult> CalculateNodeVoltages();
        void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling);
    }
}