using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public interface INodeVoltageCalculator
    {
        Vector<Complex> CalculateUnknownVoltages(
            IReadOnlyAdmittanceMatrix admittances, IList<Complex> totalAdmittanceRowSums, double nominalVoltage, 
            Vector<Complex> initialVoltages, Vector<Complex> constantCurrents, IList<PqNodeWithIndex> pqBuses, IList<PvNodeWithIndex> pvBuses);
        double MaximumRelativePowerError { get; }
        double Progress { get; }
        double RelativePowerError { get; }
        string StatusMessage { get; }
    }
}
