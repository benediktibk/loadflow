using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElementWithInternalNodes
    {
        private readonly string _name;
        private readonly IReadOnlyNode _node;
        private readonly Complex _voltage;
        private readonly double _shortCircuitPower;

        public FeedIn(string name, IReadOnlyNode node, Complex voltage, double shortCircuitPower)
        {
            if (shortCircuitPower < 0)
                throw new ArgumentOutOfRangeException("shortCircuitPower", "must not be negative");

            _name = name;
            _node = node;
            _voltage = voltage;
            _shortCircuitPower = shortCircuitPower;
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

        public double ShortCircuitPower
        {
            get { return _shortCircuitPower; }
        }

        public double InputImpedance
        {
            get
            {
                if (_shortCircuitPower == 0)
                    throw new InvalidOperationException();

                var nominalVoltage = NominalVoltage;
                return 1.1*nominalVoltage*nominalVoltage/_shortCircuitPower;
            }
        }

        public bool EnforcesSlackBus
        {
            get { return true; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, 1);
            return scaler.ScaleVoltage(Voltage);
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasisPower)
        {

        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}
