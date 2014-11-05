using System;
using System.Collections.Generic;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNet : IPowerNet
    {
        private readonly IList<INode> _nodes;
        private readonly IAdmittanceMatrix _admittances;
        private readonly double _nominalVoltage;

        public PowerNet(IAdmittanceMatrix admittances, double nominalVoltage)
        {
            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            _nominalVoltage = nominalVoltage;
            _admittances = admittances;
            _nodes = new List<INode>(_admittances.NodeCount);
        }

        public void AddNode(INode node)
        {
            _nodes.Add(node);
        }

        public IReadOnlyList<INode> Nodes
        {
            get { return (IReadOnlyList<INode>) _nodes; }
        }

        public IReadOnlyAdmittanceMatrix Admittances
        {
           get { return _admittances; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }
    }
}
