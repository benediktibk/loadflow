using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public interface IReadOnlyNode
    {
        Complex Power { get; }
        Complex Voltage { get; }
        double RealPower { get; }
        double VoltageMagnitude { get; }
        bool VoltageIsKnown { get; }
        bool PowerIsKnown { get; }
        bool VoltageMagnitudeIsKnown { get; }
        bool RealPowerIsKnown { get; }
        bool IsPQBus { get; }
        bool IsPVBus { get; }
        bool IsSlackBus { get; }
    }
}
