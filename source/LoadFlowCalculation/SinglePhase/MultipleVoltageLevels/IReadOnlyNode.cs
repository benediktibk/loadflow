using System;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        long Id { get; }
        SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
    }
}
