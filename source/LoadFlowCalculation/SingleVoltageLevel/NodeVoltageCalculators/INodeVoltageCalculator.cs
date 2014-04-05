using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators
{
    public interface INodeVoltageCalculator
    {
        Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, IList<Complex> totalAdmittanceRowSums,
            double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses);
        double GetMaximumPowerError();
    }
}
