using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class CurrentSource : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _current;
        private readonly Complex _internalImpedance;
        private readonly DerivedInternalPQNode _internalNode;

        public CurrentSource(IExternalReadOnlyNode node, Complex current, Complex internalImpedance, IdGenerator idGenerator)
        {
            if (internalImpedance.MagnitudeSquared() <= 0)
                throw new ArgumentOutOfRangeException("internalImpedance", "must not be zero");

            if (current.MagnitudeSquared() <= 0)
                throw new ArgumentOutOfRangeException("current", "must not be zero");

            _node = node;
            _current = current;
            _internalImpedance = internalImpedance;
            _internalNode = new DerivedInternalPQNode(_node, idGenerator.Generate(), new Complex(), "");
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
            get { return _node.NominalVoltage*_current.Magnitude; }
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddDirectConnectedNodes(visitedNodes);
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            return new PqNode(new Complex());
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode> {_internalNode};
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            var scaler = new DimensionScaler(_node.NominalVoltage, scaleBasePower);
            admittances.AddConnection(_internalNode, _node, scaler.ScaleAdmittance(1/_internalImpedance));
        }

        public void FillInConstantCurrents(IList<Complex> constantCurrents, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower)
        {
            var index = nodeIndices[_internalNode];
            var scaler = new DimensionScaler(_node.NominalVoltage, scaleBasePower);
            var currentScaled = scaler.ScaleCurrent(_current);
            constantCurrents[index] += currentScaled;
        }

        public bool IsConnectedTo(ISet<IExternalReadOnlyNode> nodes)
        {
            return nodes.Contains(_node);
        }
    }
}
