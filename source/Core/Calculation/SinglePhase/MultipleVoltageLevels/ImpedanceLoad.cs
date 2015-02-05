using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ImpedanceLoad : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _impedance;

        public ImpedanceLoad(IExternalReadOnlyNode node, Complex impedance)
        {
            _node = node;
            _impedance = impedance;
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public Complex Impedance
        {
            get { return _impedance; }
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public IList<Tuple<IReadOnlyNode, IReadOnlyNode>> GetDirectConnectedNodes()
        {
            return new List<Tuple<IReadOnlyNode, IReadOnlyNode>>();
        }

        public INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited, bool includeDirectConnections)
        {
            return new PqNode(new Complex());
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            var impedanceScaled = scaler.ScaleImpedance(_impedance);
            var admittanceScaled = 1.0/impedanceScaled;
            admittances.AddConnection(_node, groundNode, admittanceScaled);
        }

        public void FillInConstantCurrents(IList<Complex> constantCurrents, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower)
        { }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return true; }
        }
    }
}
