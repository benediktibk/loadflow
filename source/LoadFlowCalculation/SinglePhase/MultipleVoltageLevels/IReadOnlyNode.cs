using System;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        long Id { get; }
        SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
    }
}
