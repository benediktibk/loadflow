using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IExternalReadOnlyNode : IReadOnlyNode
    {
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
        bool IsOverdetermined { get; }
        Complex Voltage { get; }
        double NominalPhaseShift { get; }
    }
}
