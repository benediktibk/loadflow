using System;
using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _voltage;
        private readonly double _shortCircuitPower;
        private readonly DerivedInternalSlackNode _internalNode;

        public FeedIn(IExternalReadOnlyNode node, Complex voltage, double shortCircuitPower, IdGenerator idGenerator)
        {
            if (shortCircuitPower < 0)
                throw new ArgumentOutOfRangeException("shortCircuitPower", "must not be negative");

            _node = node;
            _voltage = voltage;
            _shortCircuitPower = shortCircuitPower;
            _internalNode = new DerivedInternalSlackNode(_node, idGenerator.Generate(), voltage);
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
                if (!InternalNodeNecessary)
                    throw new InvalidOperationException();

                var nominalVoltage = NominalVoltage;
                return 1.1*nominalVoltage*nominalVoltage/_shortCircuitPower;
            }
        }

        public bool InternalNodeNecessary
        {
            get { return _shortCircuitPower > 0; }
        }

        public bool EnforcesSlackBus
        {
            get { return true; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
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
            if (NominalVoltage == 0)
                return new Complex(0, 0);

            var scaler = new DimensionScaler(NominalVoltage, 1);
            return scaler.ScaleVoltage(Voltage);
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            if (!InternalNodeNecessary)
                return;

            var scaler = new DimensionScaler(NominalVoltage, scaleBasisPower);
            var admittanceScaled = scaler.ScaleAdmittance(1/InputImpedance);
            admittances.AddConnection(_internalNode, _node, admittanceScaled);
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            var result = new List<IReadOnlyNode>();

            if (InternalNodeNecessary)
                result.Add(_internalNode);

            return result;
        }
    }
}
