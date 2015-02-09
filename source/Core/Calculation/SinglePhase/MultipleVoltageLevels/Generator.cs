using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Generator : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        public Generator(IExternalReadOnlyNode node, double voltageMagnitude, double realPower)
        {
            _node = node;
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public double VoltageMagnitude
        {
            get { return _voltageMagnitude; }
        }

        public double RealPower
        {
            get { return _realPower; }
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
            get { return _realPower; }
        }

        public void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddDirectConnectedNodes(visitedNodes);
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new PvNode(scaler.ScalePower(RealPower), scaler.ScaleVoltage(VoltageMagnitude));
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
