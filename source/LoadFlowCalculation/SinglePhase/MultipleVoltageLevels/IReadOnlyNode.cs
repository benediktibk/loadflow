using System;
using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IReadOnlyNode : IEquatable<IReadOnlyNode>
    {
        double NominalVoltage { get; }
        string Name { get; }
        bool IsOverdetermined { get; }
        bool MustBeSlackBus { get; }
        bool MustBePVBus { get; }
        void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes);
        IReadOnlyCollection<IPowerNetElement> ConnectedElements { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        Complex GetTotalPowerForPQBus(double scaleBasePower);
        Complex GetSlackVoltage(double scaleBasePower);
    }
}
