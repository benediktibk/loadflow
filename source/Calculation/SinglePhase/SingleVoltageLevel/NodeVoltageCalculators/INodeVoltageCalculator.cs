using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public interface INodeVoltageCalculator
    {
        Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, IReadOnlyList<Complex> nominalVoltages, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses);
        double GetMaximumPowerError();
    }
}
