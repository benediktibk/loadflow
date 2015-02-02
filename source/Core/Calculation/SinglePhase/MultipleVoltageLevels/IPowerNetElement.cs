using System;
using System.Collections.Generic;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        bool NominalVoltagesMatch { get; }
        bool NeedsGroundNode { get; }
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes);
        IList<Tuple<IReadOnlyNode, IReadOnlyNode>> GetDirectConnectedNodes();
        INode CreateSingleVoltageNode(double scaleBasePower, IExternalReadOnlyNode source);
        IList<IReadOnlyNode> GetInternalNodes();
        void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow);
    }
}
