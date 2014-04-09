using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        IList<Node> GetConnectedNodes();
    }
}
