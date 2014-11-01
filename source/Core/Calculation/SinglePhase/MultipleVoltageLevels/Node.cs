using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Node : IExternalReadOnlyNode
    {
        private readonly List<IPowerNetElement> _connectedElements;
        private Complex _voltage;
        private bool _voltageSet;

        public Node(int id, double nominalVoltage, double nominalPhaseShift, string name)
        {
            Id = id;
            NominalVoltage = nominalVoltage;
            NominalPhaseShift = nominalPhaseShift;
            _connectedElements = new List<IPowerNetElement>();
            _voltage = new Complex();
            _voltageSet = false;
            Name = name;
        }

        public double NominalVoltage { get; private set; }

        public double NominalPhaseShift { get; private set; }

        public int Id { get; private set; }

        public Complex Voltage
        {
            get
            {
                if (!_voltageSet)
                    throw new InvalidOperationException("voltage not yet set");

                return _voltage;
            }
            set
            {
                _voltage = value;
                _voltageSet = true;
            }
        }

        public Complex Power { get; private set; }

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

        public string Name { get; private set; }

        public void Connect(IPowerNetElement element)
        {
            _connectedElements.Add(element);
        }

        public IReadOnlyCollection<IPowerNetElement> ConnectedElements
        {
            get { return _connectedElements; }
        }

        public SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            var singleVoltageNode = new SingleVoltageLevel.Node {NominalPhaseShift = NominalPhaseShift};

            if (MustBeSlackBus)
                singleVoltageNode.Voltage = GetSlackVoltage(scaleBasePower);
            else if (MustBePVBus)
            {
                var data = GetVoltageMagnitudeAndRealPowerForPVBus(scaleBasePower);
                singleVoltageNode.VoltageMagnitude = data.Item1;
                singleVoltageNode.RealPower = data.Item2;
            }
            else
                singleVoltageNode.Power = GetTotalPowerForPQBus(scaleBasePower);

            return singleVoltageNode;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            var enforcingElements = _connectedElements.Where(x => x.EnforcesPVBus).ToList();

            if (enforcingElements.Count != 1)
                throw new InvalidOperationException(
                    "can not create a PV-bus for this node as no (or more than one) element enforces the PV-bus");

            var enforcingElement = enforcingElements.First();
            var partialResult = enforcingElement.GetVoltageMagnitudeAndRealPowerForPVBus(scaleBasePower);

            var loadElements = _connectedElements.Where(x => !x.EnforcesPVBus && !x.EnforcesSlackBus);
            var additionalLoad = loadElements.Aggregate(new Complex(),
                (current, element) => current + element.GetTotalPowerForPQBus(scaleBasePower));
            var totalPower = partialResult.Item2 + additionalLoad.Real;
            var voltageMagnitude = partialResult.Item1;
            return new Tuple<double, double>(voltageMagnitude, totalPower);
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            var totalPower = new Complex();
            return _connectedElements.Aggregate(totalPower, (current, element) => current + element.GetTotalPowerForPQBus(scaleBasePower));
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            var enforcingElements = _connectedElements.Where(x => x.EnforcesSlackBus).ToList();

            if (enforcingElements.Count != 1)
                throw new InvalidOperationException(
                    "can not create a slack bus for this node as no (or more than one) element enforces the slack bus");

            var enforcingElement = enforcingElements.First();
            return enforcingElement.GetSlackVoltage(scaleBasePower);
        }

        public bool Equals(IReadOnlyNode other)
        {
            return Id == other.Id;
        }

        public void UpdateVoltageAndPower(IReadOnlyDictionary<long, NodeResult> nodeResults, double powerScaling)
        {
            Debug.Assert(nodeResults.ContainsKey(Id));
            var scaler = new DimensionScaler(NominalVoltage, powerScaling);
            var nodeResult = nodeResults[Id];
            Voltage = scaler.UnscaleVoltage(nodeResult.Voltage);
            Power = scaler.UnscalePower(nodeResult.Power);
        }
    }
}
