using System;
using System.Collections.Generic;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ExternalNode : IExternalReadOnlyNode
    {
        private readonly List<IPowerNetElement> _connectedElements;

        public ExternalNode(int id, double nominalVoltage, string name)
        {
            Id = id;
            NominalVoltage = nominalVoltage;
            _connectedElements = new List<IPowerNetElement>();
            Name = name;
        }

        public double NominalVoltage { get; private set; }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public void Connect(IPowerNetElement element)
        {
            _connectedElements.Add(element);
        }

        public IReadOnlyCollection<IPowerNetElement> ConnectedElements
        {
            get { return _connectedElements; }
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            if (visited.Contains(this))
                throw new InvalidOperationException("already visited");

            if (_connectedElements.Count == 0)
                throw new InvalidOperationException("node is not connected");

            var visitedCopy = new HashSet<IExternalReadOnlyNode>(visited) {this};
            var singleVoltageNodes = _connectedElements.Select(x => x.CreateSingleVoltageNode(scaleBasePower, visitedCopy, includeDirectConnections));
            var result = singleVoltageNodes.First();
            return singleVoltageNodes.Skip(1).Aggregate(result, (current, node) => current.Merge(node));
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddDirectConnectedNodes(visitedNodes);
        }
    }
}
