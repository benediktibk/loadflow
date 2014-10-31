using System;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        int Id { get; }
        SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
        string Name { get; }
    }
}
