using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Load : IPowerNetElement
    {
        private readonly Complex _load;
        private readonly IExternalReadOnlyNode _node;

        public Load(Complex load, IExternalReadOnlyNode node)
        {
            _load = load;
            _node = node;
        }

        public Complex Value
        {
            get { return _load; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return false; }
        }

        public double MaximumPower
        {
            get { return _load.Magnitude; }
        }

        public IList<Tuple<IReadOnlyNode, IReadOnlyNode>> GetDirectConnectedNodes()
        {
            return new List<Tuple<IReadOnlyNode, IReadOnlyNode>>();   
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new PqNode(scaler.ScalePower(Value));
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode, double expectedLoadFlow)
        { }

        public void FillInConstantCurrents(IList<Complex> constantCurrents, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower)
        { }

        public bool IsConnectedTo(ISet<IExternalReadOnlyNode> nodes)
        {
            return nodes.Contains(_node);
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}
