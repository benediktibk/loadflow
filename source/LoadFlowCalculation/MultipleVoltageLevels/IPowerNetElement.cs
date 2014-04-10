using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        void AddConnectedNodes(ISet<Node> visitedNodes);
    }
}
