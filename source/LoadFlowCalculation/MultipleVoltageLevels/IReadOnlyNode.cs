using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IPowerNetElement, IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        string Name { get; }
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
    }
}
