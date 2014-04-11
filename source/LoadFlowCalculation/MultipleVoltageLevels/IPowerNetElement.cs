using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes);
        bool EnforcesSlackBus { get; }
        bool EnforcesPVBus { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasisPower);
        Complex GetTotalPowerForPQBus(double scaleBasisPower);
        Complex GetSlackVoltage();
    }
}
