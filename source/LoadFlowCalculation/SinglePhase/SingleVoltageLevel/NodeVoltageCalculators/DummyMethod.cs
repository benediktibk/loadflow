using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class DummyMethod : INodeVoltageCalculator
    {
        public Vector<Complex> CalculateUnknownVoltages(AdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses, IList<PVBus> pvBuses)
        {
            return DenseVector.Create(admittances.NodeCount, i => new Complex(0, 0));
        }

        public double GetMaximumPowerError()
        {
            return 0;
        }
    }
}
