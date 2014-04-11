using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Load : IPowerNetElement
    {
        private readonly Complex _load;
        private readonly string _name;
        private readonly IReadOnlyNode _node;

        public Load(string name, Complex load, IReadOnlyNode node)
        {
            _load = load;
            _name = name;
            _node = node;
        }

        public Complex Value
        {
            get { return _load; }
        }

        public string Name
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public bool EnforcesSlackBus
        {
            get { return false; }
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
            var scaler = new DimensionSingleLevelScaler(NominalVoltage, 1);
            return scaler.ScalePower(Value);
        }

        public Complex GetSlackVoltage(double scaleBasisVoltage)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
