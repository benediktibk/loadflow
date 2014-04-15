using System;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        string Name { get; }
        bool MustBeSlackBus { get; }
        bool MustBePVBus { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        Complex GetTotalPowerForPQBus(double scaleBasePower);
        Complex GetSlackVoltage(double scaleBasePower);
    }
}
