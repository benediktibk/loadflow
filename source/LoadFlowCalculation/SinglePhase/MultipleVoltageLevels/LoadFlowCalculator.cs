﻿using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
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

        public IDictionary<string, Complex> CalculateNodeVoltages(IReadOnlyPowerNet powerNet)
        {
            CheckPowerNet(powerNet);

            var nodes = powerNet.GetNodes();
            var lines = powerNet.GetLines();
            var nodeIndexes = DetermineNodeIndexes(nodes);
            var admittanes = CalculateAdmittanceMatrix(nodes, lines, nodeIndexes);
            var singleVoltageNodes = CreateSingleVoltageNodes(nodes, nodeIndexes);

            var calculator = new SingleVoltageLevel.LoadFlowCalculator(_nodeVoltageCalculator);
            bool voltageCollapse;
            var singleVoltageNodesWithResults = calculator.CalculateNodeVoltagesAndPowers(admittanes, 1,
                singleVoltageNodes, out voltageCollapse);

            return ExtractNodeVoltages(nodes, nodeIndexes, singleVoltageNodesWithResults);
        }

        private static Dictionary<string, Complex> ExtractNodeVoltages(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes,
            IList<SingleVoltageLevel.Node> singleVoltageNodesWithResults)
        {
            var nodeVoltages = new Dictionary<string, Complex>();

            foreach (var node in nodes)
            {
                var index = nodeIndexes[node];
                var name = node.Name;
                var voltage = singleVoltageNodesWithResults[index].Voltage*node.NominalVoltage;
                nodeVoltages.Add(name, voltage);
            }
            return nodeVoltages;
        }

        private SingleVoltageLevel.Node[] CreateSingleVoltageNodes(IReadOnlyList<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            var singleVoltageNodes = new SingleVoltageLevel.Node[nodes.Count];
            foreach (var node in nodes)
            {
                var singleVoltageNode = new SingleVoltageLevel.Node();

                if (node.MustBeSlackBus)
                    singleVoltageNode.Voltage = node.GetSlackVoltage(ScaleBasePower);
                else if (node.MustBePVBus)
                {
                    var data = node.GetVoltageMagnitudeAndRealPowerForPVBus(ScaleBasePower);
                    singleVoltageNode.VoltageMagnitude = data.Item1;
                    singleVoltageNode.RealPower = data.Item2;
                }
                else
                    singleVoltageNode.Power = node.GetTotalPowerForPQBus(ScaleBasePower);

                var nodeIndex = nodeIndexes[node];
                singleVoltageNodes[nodeIndex] = singleVoltageNode;
            }
            return singleVoltageNodes;
        }

        private SparseMatrix CalculateAdmittanceMatrix(IReadOnlyCollection<IReadOnlyNode> nodes, IEnumerable<Line> lines, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes)
        {
            var admittanes = new SparseMatrix(nodes.Count, nodes.Count);
            foreach (var line in lines)
                line.FillInAdmittances(admittanes, nodeIndexes, ScaleBasePower);
            return admittanes;
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