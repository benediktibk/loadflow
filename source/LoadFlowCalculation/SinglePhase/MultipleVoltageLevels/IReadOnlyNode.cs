using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>, IPowerNetElement
    {
        double NominalVoltage { get; }
        string Name { get; }
        bool IsOverdetermined { get; }
        bool MustBeSlackBus { get; }
        bool MustBePVBus { get; }
        IReadOnlyCollection<IPowerNetElementWithInternalNodes> ConnectedElements { get; }
    }
}
