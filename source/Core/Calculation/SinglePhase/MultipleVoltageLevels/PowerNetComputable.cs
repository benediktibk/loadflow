﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNetComputable : PowerNet
    {
        private INodeVoltageCalculator _nodeVoltageCalculator;

        public PowerNetComputable(INodeVoltageCalculator nodeVoltageCalculator, double frequency) : base(frequency)
        {
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public INodeVoltageCalculator NodeVoltageCalculator
        {
            get { return _nodeVoltageCalculator; }
            set { _nodeVoltageCalculator = value; }
        }

        public IReadOnlyDictionary<long, NodeResult> CalculateNodeVoltages()
        {
            CheckPowerNet();

            var powerScaling = DeterminePowerScaling();
            var nodes = new List<IReadOnlyNode>(GetAllNecessaryNodes());
            var nodeIndexes = DetermineNodeIndexes(nodes);
            var admittances = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerScaling);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(nodes, nodeIndexes, admittances, powerScaling);
            var nodeResults = singleVoltagePowerNet.CalculateNodeResults();

            if (nodeResults == null)
                return null;

            var nodeResultsWithId = new Dictionary<long, NodeResult>();

            foreach (var node in Nodes)
            {
                var nodeIndex = nodeIndexes[node];
                var nodeResult = nodeResults[nodeIndex];
                var id = node.Id;
                var nodeResultUnscaled = nodeResult.Unscale(node.NominalVoltage, powerScaling);
                nodeResultsWithId.Add(id, nodeResultUnscaled);
            }

            return nodeResultsWithId;
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, double powerScaling)
        {
            var nodes = new List<IReadOnlyNode>(GetAllNecessaryNodes());
            var nodeIndexes = DetermineNodeIndexes(nodes);
            matrix = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerScaling);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private SingleVoltageLevel.PowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, IAdmittanceMatrix admittances, double scaleBasePower)
        {
            var singleVoltagePowerNet = new SingleVoltageLevel.PowerNetComputable(_nodeVoltageCalculator, admittances.SingleVoltageAdmittanceMatrix, 1);

            foreach (var node in nodes)
            {
                var singleVoltageNode = node.CreateSingleVoltageNode(scaleBasePower);
                var nodeIndex = nodeIndexes[node];
                singleVoltagePowerNet.SetNode(nodeIndex, singleVoltageNode);
            }

            return singleVoltagePowerNet;
        }

        private AdmittanceMatrix CalculateAdmittanceMatrix(IReadOnlyCollection<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasePower)
        {
            var admittances = new AdmittanceMatrix(new SingleVoltageLevel.AdmittanceMatrix(nodes.Count), nodeIndexes);
            FillInAdmittances(admittances, scaleBasePower);
            return admittances;
        }

        private static Dictionary<IReadOnlyNode, int> DetermineNodeIndexes(IReadOnlyList<IReadOnlyNode> nodes)
        {
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>();

            for (var i = 0; i < nodes.Count; ++i)
                nodeIndexes.Add(nodes[i], i);

            return nodeIndexes;
        }

        private void CheckPowerNet()
        {
            if (CheckIfFloatingNodesExists())
                throw new InvalidDataException("there must not be a floating node");
            if (CheckIfNominalVoltagesDoNotMatch())
                throw new InvalidDataException("the nominal voltages must match on connected nodes");
            if (CheckIfNodeIsOverdetermined())
                throw new InvalidDataException("one node is overdetermined");
        }
    }
}