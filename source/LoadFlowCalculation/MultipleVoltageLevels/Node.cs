using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

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

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasisVoltage, double scaleBasisPower)
        {
            var enforcingElements = _connectedElements.Where(x => x.EnforcesPVBus).ToList();

            if (enforcingElements.Count != 1)
                throw new InvalidOperationException(
                    "can not create a PV-bus for this node as no (or more than one) element enforces the PV-bus");

            var enforcingElement = enforcingElements.First();
            var partialResult = enforcingElement.GetVoltageMagnitudeAndRealPowerForPVBus(scaleBasisVoltage, scaleBasisPower);

            var loadElements = _connectedElements.Where(x => !x.EnforcesPVBus && !x.EnforcesSlackBus);
            var additionalLoad = loadElements.Aggregate(new Complex(),
                (current, element) => current + element.GetTotalPowerForPQBus(scaleBasisPower));
            var totalPower = partialResult.Item2 + additionalLoad.Real;
            var voltageMagnitude = partialResult.Item1;
            return new Tuple<double, double>(voltageMagnitude, totalPower);
        }

        public Complex GetTotalPowerForPQBus(double scaleBasisPower)
        {
            var totalPower = new Complex();
            return _connectedElements.Aggregate(totalPower, (current, element) => current + element.GetTotalPowerForPQBus(scaleBasisPower));
        }

        public Complex GetSlackVoltage(double scaleBasisVoltage)
        {
            var enforcingElements = _connectedElements.Where(x => x.EnforcesSlackBus).ToList();

            if (enforcingElements.Count != 1)
                throw new InvalidOperationException(
                    "can not create a slack bus for this node as no (or more than one) element enforces the slack bus");

            var enforcingElement = enforcingElements.First();
            return enforcingElement.GetSlackVoltage(scaleBasisVoltage);
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
