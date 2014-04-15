using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly string _name;
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _voltage;
        private readonly double _shortCircuitPower;
        private readonly DerivedInternalSlackNode _internalNode;

        public FeedIn(string name, IExternalReadOnlyNode node, Complex voltage, double shortCircuitPower)
        {
            if (shortCircuitPower < 0)
                throw new ArgumentOutOfRangeException("shortCircuitPower", "must not be negative");

            _name = name;
            _node = node;
            _voltage = voltage;
            _shortCircuitPower = shortCircuitPower;
            _internalNode = new DerivedInternalSlackNode(_node, name + "#" + "internal", voltage);
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

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasisPower)
        {
            if (!InternalNodeNecessary)
                return;

            var scaler = new DimensionScaler(NominalVoltage, scaleBasisPower);
            var admittanceScaled = scaler.ScaleAdmittance(1/InputImpedance);
            var internalIndex = nodeIndexes[_internalNode];
            var externalIndex = nodeIndexes[_node];
            admittances[internalIndex, internalIndex] += admittanceScaled;
            admittances[externalIndex, externalIndex] += admittanceScaled;
            admittances[externalIndex, internalIndex] -= admittanceScaled;
            admittances[internalIndex, externalIndex] -= admittanceScaled;
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
