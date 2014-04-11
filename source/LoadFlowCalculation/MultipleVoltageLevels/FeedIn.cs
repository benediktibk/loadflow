using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _node;
        private readonly Complex _voltage;

        public FeedIn(string name, IReadOnlyNode node, Complex voltage)
        {
            _name = name;
            _node = node;
            _voltage = voltage;
        }

        public string Name
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public Complex Voltage
        {
            get { return _voltage; }
        }

        public bool EnforcesSlackBus
        {
            get { return true; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasisVoltage, double scaleBasisPower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasisPower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasisVoltage)
        {
            return Voltage/scaleBasisVoltage;
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
