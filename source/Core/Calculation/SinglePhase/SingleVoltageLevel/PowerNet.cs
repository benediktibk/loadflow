using System;
using System.Collections.Generic;
using System.Linq;

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
            _nodes = InitializeNodes();
        }

        public void SetNode(int i, INode node)
        {
            _nodes[i] = node;
        }

        public IReadOnlyList<INode> Nodes
        {
            get { return _nodes.Cast<INode>().ToList(); }
        }

        public IReadOnlyAdmittanceMatrix Admittances
        {
           get { return _admittances; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }

        private IList<INode> InitializeNodes()
        {
            var result = new List<INode>(Admittances.NodeCount);

            for (var i = 0; i < Admittances.NodeCount; ++i)
                result.Add(null);

            return result;
        }
    }
}
