using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Node : IEquatable<Node>, INode
    {
        private readonly string _name;
        private readonly double _nominalVoltage;
        private readonly List<IPowerNetElement> _connectedElements; 

        public Node(string name, double nominalVoltage)
        {
            _name = name;
            _nominalVoltage = nominalVoltage;
            _connectedElements = new List<IPowerNetElement>();
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
            return _name.GetHashCode();
        }

        public void AddConnectedNodes(ISet<INode> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddConnectedNodes(visitedNodes);
        }

        public bool Equals(Node other)
        {
            return _name == other.Name;
        }
    }
}
