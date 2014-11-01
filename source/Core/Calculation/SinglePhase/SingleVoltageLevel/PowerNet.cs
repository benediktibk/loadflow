using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNet
    {
        private readonly int _nodeCount;
        private readonly IList<Node> _nodes;
        private readonly AdmittanceMatrix _admittances;
        private readonly double _nominalVoltage;

        public PowerNet(AdmittanceMatrix admittances, double nominalVoltage)
        {
            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            _nominalVoltage = nominalVoltage;
            _nodeCount = admittances.NodeCount;
            _admittances = admittances;
            _nodes = InitializeNodes();
        }

        public void SetNodeResults(IList<NodeResult> nodeResults)
        {
            for (var i = 0; i < NodeCount; ++i)
            {
                _nodes[i].Voltage = nodeResults[i].Voltage;
                _nodes[i].Power = nodeResults[i].Power;
            }
        }

        public void SetNode(int i, Node node)
        {
            _nodes[i] = node;
        }

        public bool IsVoltageOfNodeKnown(int node)
        {
            return _nodes[node].VoltageIsKnown;
        }

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IReadOnlyNode>().ToList(); }
        }

        public IReadOnlyAdmittanceMatrix Admittances
        {
           get { return _admittances; }
        }

        public int NodeCount
        {
            get { return _nodeCount; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }

        private IList<Node> InitializeNodes()
        {
            var result = new List<Node>(_nodeCount);

            for (var i = 0; i < _nodeCount; ++i)
                result.Add(new Node());

            return result;
        }
    }
}
