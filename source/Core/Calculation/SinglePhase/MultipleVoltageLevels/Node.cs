using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using System.Collections.Generic;
using System.Linq;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Node : IExternalReadOnlyNode
    {
        private readonly List<IPowerNetElement> _connectedElements;

        public Node(int id, double nominalVoltage, string name)
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

        public INode CreateSingleVoltageNode(double scaleBasePower)
        {
            var singleVoltageNodes = _connectedElements.Select(x => x.CreateSingleVoltageNode(scaleBasePower));
            return singleVoltageNodes.Aggregate((INode) new PqNode(new Complex()), (current, singleVoltageNode) => current.Merge(singleVoltageNode));
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
    }
}
