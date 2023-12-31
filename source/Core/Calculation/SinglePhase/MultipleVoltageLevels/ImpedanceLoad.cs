﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ImpedanceLoad : IPowerNetElement
    {
        private readonly IExternalReadOnlyNode _node;
        private readonly Complex _impedance;

        public ImpedanceLoad(IExternalReadOnlyNode node, Complex impedance)
        {
            var magnitude = impedance.Magnitude;

            if (magnitude <= 0 || Double.IsNaN(magnitude))
                throw new ArgumentOutOfRangeException("impedance");

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

        public double MaximumPower
        {
            get { return _node.NominalVoltage*_node.NominalVoltage/_impedance.Magnitude; }
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
        }

        public void AddDirectConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddDirectConnectedNodes(visitedNodes);
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

        public bool IsConnectedTo(ISet<IExternalReadOnlyNode> nodes)
        {
            return nodes.Contains(_node);
        }

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
