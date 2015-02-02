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
        private readonly Complex _internalImpedance;
        private readonly DerivedInternalSlackNode _internalNode;

        public FeedIn(IExternalReadOnlyNode node, Complex voltage, Complex internalImpedance, IdGenerator idGenerator)
        {
            _node = node;
            _voltage = voltage;
            _internalImpedance = internalImpedance;
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

        public Complex InternalImpedance
        {
            get { return _internalImpedance; }
        }

        public bool InternalNodeNecessary
        {
            get { return _internalImpedance.Magnitude > 0; }
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

        public IList<Tuple<IReadOnlyNode, IReadOnlyNode>> GetDirectConnectedNodes()
        {
            return new List<Tuple<IReadOnlyNode, IReadOnlyNode>>();
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, IExternalReadOnlyNode source)
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
            var admittanceScaled = scaler.ScaleAdmittance(1/InternalImpedance);
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
