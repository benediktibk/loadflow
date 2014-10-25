using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes);
        bool EnforcesSlackBus { get; }
        bool EnforcesPVBus { get; }
        Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        Complex GetTotalPowerForPQBus(double scaleBasePower);
        Complex GetSlackVoltage(double scaleBasePower);
        IList<IReadOnlyNode> GetInternalNodes();
        void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow);
        bool NominalVoltagesMatch { get; }
        bool NeedsGroundNode { get; }
    }
}
