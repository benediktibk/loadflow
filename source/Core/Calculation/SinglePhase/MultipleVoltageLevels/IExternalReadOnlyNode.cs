using System.Collections.Generic;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IExternalReadOnlyNode : IReadOnlyNode
    {
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
    }
}
