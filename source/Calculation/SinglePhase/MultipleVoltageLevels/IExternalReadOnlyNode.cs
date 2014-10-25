using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IExternalReadOnlyNode : IReadOnlyNode
    {
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes);
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
        bool IsOverdetermined { get; }
        Complex Voltage { get; }
        Complex Power { get; }
        double NominalPhaseShift { get; }
    }
}
