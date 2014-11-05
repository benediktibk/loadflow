using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode
    {
        double NominalVoltage { get; }
        int Id { get; }
        INode CreateSingleVoltageNode(double scaleBasePower);
        string Name { get; }
    }
}
