using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Node : IReadOnlyNode
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

        public bool IsOverdetermined
        {
            get { return _connectedElements.Count(element => element.EnforcesSlackBus) + _connectedElements.Count(element => element.EnforcesPVBus) > 1; }
        }

        public bool MustBeSlackBus
        {
            get { return _connectedElements.Count(element => element.EnforcesSlackBus) > 0; }
        }

        public bool MustBePVBus
        {
            get { return _connectedElements.Count(element => element.EnforcesPVBus) > 0; }
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(this))
                return;

            visitedNodes.Add(this);
            foreach (var element in _connectedElements)
                element.AddConnectedNodes(visitedNodes);
        }

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public bool Equals(IReadOnlyNode other)
        {
            return _name == other.Name;
        }
    }
}
