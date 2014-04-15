using System;
using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes);
        bool EnforcesSlackBus { get; }
        bool EnforcesPVBus { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        Complex GetTotalPowerForPQBus(double scaleBasePower);
        Complex GetSlackVoltage(double scaleBasePower);
    }
}
