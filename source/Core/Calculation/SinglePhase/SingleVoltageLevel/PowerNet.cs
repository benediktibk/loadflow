using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PowerNet : IPowerNet
    {
        private readonly IList<INode> _nodes;
        private readonly IAdmittanceMatrix _admittances;
        private readonly double _nominalVoltage;
        private readonly IReadOnlyList<Complex> _constantCurrents;

        public PowerNet(IAdmittanceMatrix admittances, double nominalVoltage, IReadOnlyList<Complex> constantCurrents)
        {
            if (nominalVoltage <= 0)
                throw new ArgumentOutOfRangeException("nominalVoltage", "the nominal voltage must be positive");

            if (admittances.NodeCount != constantCurrents.Count)
                throw new ArgumentException("dimensions do not match");

            _nominalVoltage = nominalVoltage;
            _admittances = admittances;
            _nodes = new List<INode>(_admittances.NodeCount);
            _constantCurrents = constantCurrents;
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

        public IReadOnlyList<Complex> ConstantCurrents
        {
            get { return _constantCurrents; }
        }
    }
}
