using System;
using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Generator : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _node;
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        public Generator(string name, IReadOnlyNode node, double voltageMagnitude, double realPower)
        {
            _name = name;
            _node = node;
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        public string Name 
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public double VoltageMagnitude
        {
            get { return _voltageMagnitude; }
        }

        public double RealPower
        {
            get { return _realPower; }
        }

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return true; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            var scaler = new DimensionSingleLevelScaler(NominalVoltage, scaleBasePower);
            return new Tuple<double, double>(scaler.ScaleVoltage(VoltageMagnitude), scaler.ScalePower(RealPower));
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
