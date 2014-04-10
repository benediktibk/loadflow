using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface INode : IPowerNetElement
    {
        double NominalVoltage { get; }
        string Name { get; }
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
    }
}
