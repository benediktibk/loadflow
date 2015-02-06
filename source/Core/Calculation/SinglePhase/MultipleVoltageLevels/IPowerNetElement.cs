using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNetElement
    {
        bool NominalVoltagesMatch { get; }
        bool NeedsGroundNode { get; }
        double MaximumPower { get; }
        void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes);
        void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes);
        IList<Tuple<IReadOnlyNode, IReadOnlyNode>> GetDirectConnectedNodes();
        INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections);
        IList<IReadOnlyNode> GetInternalNodes();
        void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode, double expectedLoadFlow);
        void FillInConstantCurrents(IList<Complex> constantCurrents, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower);
    }
}
