using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IPowerNetFactory
    {
        IPowerNetComputable Create(IAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyList<Complex> constantCurrents);
    }
}