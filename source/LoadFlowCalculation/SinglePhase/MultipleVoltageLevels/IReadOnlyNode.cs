using System;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        string Name { get; }
        SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
    }
}
