using System.Collections.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IExternalReadOnlyNode : IReadOnlyNode
    {
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
        bool IsOverdetermined { get; }
    }
}
