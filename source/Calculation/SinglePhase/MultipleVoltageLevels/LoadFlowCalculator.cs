﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class LoadFlowCalculator
    {
        #region variables

        private readonly double _scaleBasePower;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        #endregion

        #region public functions

        public LoadFlowCalculator(double scaleBasePower, INodeVoltageCalculator nodeVoltageCalculator)
        {
            _scaleBasePower = scaleBasePower;
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IReadOnlyDictionary<long, Complex> CalculateNodeVoltages(IReadOnlyPowerNet powerNet)
        {
            CheckPowerNet(powerNet);

            var nodes = new List<IReadOnlyNode>(powerNet.GetAllNecessaryNodes());
            var nodeIndexes = DetermineNodeIndexes(nodes);
            var admittances = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerNet);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(nodes, nodeIndexes, admittances);
            var calculator = new SingleVoltageLevel.LoadFlowCalculator(_nodeVoltageCalculator);
            var voltageCollapse = singleVoltagePowerNet.CalculateMissingInformation(calculator);
            return voltageCollapse ? null : ExtractNodeVoltages(nodes, nodeIndexes, singleVoltagePowerNet.GetNodes());
        }

        private static Dictionary<long, Complex> ExtractNodeVoltages(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes,
            IReadOnlyList<SingleVoltageLevel.Node> singleVoltageNodesWithResults)
        {
            var nodeVoltages = new Dictionary<long, Complex>();

            foreach (var node in nodes)
            {
                var index = nodeIndexes[node];
                var name = node.Id;
                var voltage = singleVoltageNodesWithResults[index].Voltage;
                nodeVoltages.Add(name, voltage);
            }

            return nodeVoltages;
        }

        private SingleVoltageLevel.PowerNet CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, IAdmittanceMatrix admittances)
        {
            var singleVoltagePowerNet = new SingleVoltageLevel.PowerNet(admittances.GetSingleVoltageAdmittanceMatrix(), 1);

            foreach (var node in nodes)
            {
                var singleVoltageNode = node.CreateSingleVoltageNode(ScaleBasePower);
                var nodeIndex = nodeIndexes[node];
                singleVoltagePowerNet.SetNode(nodeIndex, singleVoltageNode);
            }

            return singleVoltagePowerNet;
        }

        private AdmittanceMatrix CalculateAdmittanceMatrix(IReadOnlyCollection<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, IReadOnlyPowerNet powerNet)
        {
            var admittances = new AdmittanceMatrix(nodes.Count, nodeIndexes);
            powerNet.FillInAdmittances(admittances, ScaleBasePower);
            return admittances;
        }

        private static Dictionary<IReadOnlyNode, int> DetermineNodeIndexes(IReadOnlyList<IReadOnlyNode> nodes)
        {
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>();

            for (var i = 0; i < nodes.Count; ++i)
                nodeIndexes.Add(nodes[i], i);

            return nodeIndexes;
        }

        private static void CheckPowerNet(IReadOnlyPowerNet powerNet)
        {
            if (powerNet.CheckIfFloatingNodesExists())
                throw new ArgumentOutOfRangeException("powerNet", "there must not be a floating node");
            if (powerNet.CheckIfNominalVoltagesDoNotMatch())
                throw new ArgumentOutOfRangeException("powerNet", "the nominal voltages must match on connected nodes");
            if (powerNet.CheckIfNodeIsOverdetermined())
                throw new ArgumentOutOfRangeException("powerNet", "one node is overdetermined");
        }

        #endregion

        #region public properties

        public double ScaleBasePower
        {
            get { return _scaleBasePower; }
        }

        #endregion

        #region private functions
        #endregion
    }
}