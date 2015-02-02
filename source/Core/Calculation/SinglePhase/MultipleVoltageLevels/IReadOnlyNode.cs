using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode
    {
        double NominalVoltage { get; }
        int Id { get; }
        INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections);
        string Name { get; }
    }
}
