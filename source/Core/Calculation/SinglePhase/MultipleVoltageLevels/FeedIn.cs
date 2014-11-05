using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _voltage;
        private readonly double _shortCircuitPower;
        private readonly DerivedInternalSlackNode _internalNode;
        private readonly double _c;
        private readonly double _realToImaginary;

        public FeedIn(IExternalReadOnlyNode node, Complex voltage, double shortCircuitPower, double c, double realToImaginary, IdGenerator idGenerator)
        {
            if (shortCircuitPower < 0)
                throw new ArgumentOutOfRangeException("shortCircuitPower", "must not be negative");

            _node = node;
            _voltage = voltage;
            _shortCircuitPower = shortCircuitPower;
            _c = c;
            _realToImaginary = realToImaginary;
            _internalNode = new DerivedInternalSlackNode(_node, idGenerator.Generate(), voltage, "");
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

        public Complex InputImpedance
        {
            get
            {
                if (!InternalNodeNecessary)
                    throw new InvalidOperationException();

                var nominalVoltage = NominalVoltage;
                var Z = _c*nominalVoltage*nominalVoltage/_shortCircuitPower;
                var X = Math.Sqrt(_realToImaginary*_realToImaginary + 1)/Z;
                var R = _realToImaginary*X;
                return new Complex(R, X);
            }
        }

        public bool InternalNodeNecessary
        {
            get { return _shortCircuitPower > 0; }
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return false; }
        }

        public IExternalReadOnlyNode Node
        {
            get { return _node; }
        }

        public INode CreateSingleVoltageNode(double scaleBasePower)
        {
            if (NominalVoltage == 0)
                return new SlackNode(new Complex(0, 0));

            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SlackNode(scaler.ScaleVoltage(Voltage));
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
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
