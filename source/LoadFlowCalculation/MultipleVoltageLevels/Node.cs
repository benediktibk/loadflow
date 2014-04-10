using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Node : IEquatable<Node>, IPowerNetElement
    {
        private readonly string _name;
        private readonly double _nominalVoltage;
        private readonly List<IPowerNetElement> _connectedElements; 

        public Guid Id { get; private set; }

        public Node(string name, double nominalVoltage)
        {
            _name = name;
            _nominalVoltage = nominalVoltage;
            _connectedElements = new List<IPowerNetElement>();
            Id = Guid.NewGuid();
        }

        public void Connect(IPowerNetElement element)
        {
            _connectedElements.Add(element);
        }

        public IReadOnlyCollection<IPowerNetElement> ConnectedElements
        {
            get { return _connectedElements; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }

        public string Name
        {
            get { return _name; }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void AddConnectedNodes(ISet<Node> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddConnectedNodes(visitedNodes);
        }

        public bool Equals(Node other)
        {
            return GetHashCode() == other.GetHashCode();
        }
    }
}
