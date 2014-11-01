using System;
using System.Collections.Generic;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class LoadFlowCalculator
    {
        private readonly double _scaleBasePower;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        public LoadFlowCalculator(double scaleBasePower, INodeVoltageCalculator nodeVoltageCalculator)
        {
            _scaleBasePower = scaleBasePower;
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IReadOnlyDictionary<long, NodeResult> CalculateNodeVoltages(IReadOnlyPowerNet powerNet)
        {
            CheckPowerNet(powerNet);

            var nodes = new List<IReadOnlyNode>(powerNet.GetAllNecessaryNodes());
            var nodeIndexes = DetermineNodeIndexes(nodes);
            var admittances = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerNet);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(nodes, nodeIndexes, admittances);
            var nodeResults = singleVoltagePowerNet.CalculateMissingInformation();
            return nodeResults == null ? null : ExtractNodeResults(nodes, nodeIndexes, nodeResults);
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, IReadOnlyPowerNet powerNet)
        {
            var nodes = new List<IReadOnlyNode>(powerNet.GetAllNecessaryNodes());
            var nodeIndexes = DetermineNodeIndexes(nodes);
            matrix = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerNet);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        public double ScaleBasePower
        {
            get { return _scaleBasePower; }
        }

        private SingleVoltageLevel.PowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, IAdmittanceMatrix admittances)
        {
            var singleVoltagePowerNet = new SingleVoltageLevel.PowerNetComputable(_nodeVoltageCalculator, admittances.GetSingleVoltageAdmittanceMatrix(), 1);

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

        private static Dictionary<long, NodeResult> ExtractNodeResults(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, IList<NodeResult> nodeResults)
        {
            var nodeVoltages = new Dictionary<long, NodeResult>();

            foreach (var node in nodes)
            {
                var index = nodeIndexes[node];
                var name = node.Id;
                var nodeResult = nodeResults[index];
                nodeVoltages.Add(name, nodeResult);
            }

            return nodeVoltages;
        }
    }
}
