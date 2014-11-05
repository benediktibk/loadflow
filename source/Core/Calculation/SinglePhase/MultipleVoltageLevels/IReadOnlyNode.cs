using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode
    {
        double NominalVoltage { get; }
        int Id { get; }
        IEnumerable<INode> CreateSingleVoltageNodes(double scaleBasePower);
        string Name { get; }
    }
}
