using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNetComputable : PowerNet, IPowerNetComputable
    {
        private readonly IPowerNetFactory _singleVoltagePowerNetFactory;

        public PowerNetComputable(double frequency, IPowerNetFactory singleVoltagePowerNetFactory, INodeGraph nodeGraph) : base(frequency, nodeGraph)
        {
            _singleVoltagePowerNetFactory = singleVoltagePowerNetFactory;
        }

        public IReadOnlyDictionary<int, NodeResult> CalculateNodeResults(out double relativePowerError)
        {
            if (NodeGraph.FloatingNodesExist)
                throw new InvalidDataException("there must not be a floating node");
            if (NominalVoltagesDoNotMatch)
                throw new InvalidDataException("the nominal voltages must match on connected nodes");

            var powerScaling = DeterminePowerScaling();
            var nodes = GetAllCalculationNodes();
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            var admittances = CalculateAdmittanceMatrix(nodeIndices, powerScaling, mainNodes.Count);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(mainNodes, admittances, powerScaling);
            var nodeResults = singleVoltagePowerNet.CalculateNodeResults(out relativePowerError);

            if (nodeResults == null)
                return null;

            var nodeResultsWithId = new Dictionary<int, NodeResult>();

            foreach (var node in ExternalNodes)
            {
                var nodeIndex = nodeIndices[node];
                var nodeResult = nodeResults[nodeIndex];
                var id = node.Id;
                var nodeResultUnscaled = nodeResult.Unscale(node.NominalVoltage, powerScaling);
                nodeResultsWithId.Add(id, nodeResultUnscaled);
            }

            return nodeResultsWithId;
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling)
        {
            powerScaling = DeterminePowerScaling();
            var nodes = GetAllCalculationNodes();
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            matrix = CalculateAdmittanceMatrix(nodeIndices, powerScaling, mainNodes.Count);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private Dictionary<IReadOnlyNode, int> DetermineNodeIndices(IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes, IReadOnlyList<IReadOnlyNode> mainNodes, IEnumerable<IReadOnlyNode> replacableNodes)
        {
            var indices = new Dictionary<IReadOnlyNode, int>();

            for (var i = 0; i < mainNodes.Count; ++i)
            {
                var node = mainNodes[i];
                indices.Add(node, i);
            }

            foreach (var replacableNode in replacableNodes)
                indices.Add(replacableNode, indices[directConnectedNodes[replacableNode]]);

            return indices;
        }

        private static IEnumerable<IReadOnlyNode> SelectReplacableNodes(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes)
        {
            return nodes.Where(directConnectedNodes.ContainsKey);
        }

        private static IReadOnlyList<IReadOnlyNode> SelectMainNodes(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes)
        {
            return nodes.Where(x => !directConnectedNodes.ContainsKey(x)).ToList();
        }

        private SingleVoltageLevel.IPowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IAdmittanceMatrix admittances, double scaleBasePower)
        {
            var singleVoltagePowerNet = _singleVoltagePowerNetFactory.Create(admittances.SingleVoltageAdmittanceMatrix, 1);

            foreach (var node in nodes)
                singleVoltagePowerNet.AddNode(node.CreateSingleVoltageNode(scaleBasePower, new HashSet<IExternalReadOnlyNode>(), true));

            return singleVoltagePowerNet;
        }

        private AdmittanceMatrix CalculateAdmittanceMatrix(IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower, int nodeCount)
        {
            var admittances = new AdmittanceMatrix(new SingleVoltageLevel.AdmittanceMatrix(nodeCount), nodeIndices);
            FillInAdmittances(admittances, scaleBasePower);
            return admittances;
        }
    }
}
