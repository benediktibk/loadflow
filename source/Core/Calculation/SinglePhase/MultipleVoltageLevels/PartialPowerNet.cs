using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    class PartialPowerNet
    {
        private readonly IReadOnlyList<IExternalReadOnlyNode> _nodes;
        private readonly IReadOnlyList<IPowerNetElement> _elements;

        public PartialPowerNet(IReadOnlyList<IExternalReadOnlyNode> nodes, IReadOnlyList<IPowerNetElement> elements)
        {
            _nodes = nodes;
            _elements = elements;
        }

        public static Dictionary<IReadOnlyNode, int> DetermineNodeIndices(IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes, IReadOnlyList<IReadOnlyNode> mainNodes, IEnumerable<IReadOnlyNode> replacableNodes)
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

        public static IReadOnlyList<IReadOnlyNode> SelectReplacableNodes(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes)
        {
            return nodes.Where(directConnectedNodes.ContainsKey).ToList();
        }

        public static IReadOnlyList<IReadOnlyNode> SelectMainNodes(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes)
        {
            return nodes.Where(x => !directConnectedNodes.ContainsKey(x)).ToList();
        }

        public static Dictionary<int, NodeResult> CreateNodeResultsWithId(IEnumerable<KeyValuePair<IReadOnlyNode, IReadOnlyNode>> directConnectedNodes, IEnumerable<IReadOnlyNode> replacableNodes,
            IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, IList<NodeResult> nodeResults, double powerScaling, IEnumerable<IExternalReadOnlyNode> externalNodes)
        {
            var nodeResultsWithId = new Dictionary<int, NodeResult>();
            var directConnectionsReverse = directConnectedNodes.ToDictionary(connection => connection.Value,
                connection => connection.Key);
            var replacableNodesSet = new HashSet<IReadOnlyNode>();

            foreach (var replacableNode in replacableNodes)
                replacableNodesSet.Add(replacableNode);

            var externalMainNodes = externalNodes.Where(x => !replacableNodesSet.Contains(x));
            foreach (var node in externalMainNodes)
            {
                var nodeIndex = nodeIndices[node];
                var nodeResult = nodeResults[nodeIndex];
                var id = node.Id;
                var nodeResultUnscaled = nodeResult.Unscale(node.NominalVoltage, powerScaling);
                IReadOnlyNode replacableNode;

                if (!directConnectionsReverse.TryGetValue(node, out replacableNode))
                    nodeResultsWithId.Add(id, nodeResultUnscaled);
                else
                    AddNodeResultsWithIdForDirectConnection(node, replacableNode, nodeResultsWithId, nodeResultUnscaled, id);
            }

            return nodeResultsWithId;
        }

        public static void AddNodeResultsWithIdForDirectConnection(IReadOnlyNode node, IReadOnlyNode replacableNode,
            IDictionary<int, NodeResult> nodeResultsWithId, NodeResult nodeResultUnscaled, int id)
        {
            var firstSingleVoltageNode = node.CreateSingleVoltageNode(1,
                new HashSet<IExternalReadOnlyNode>(), false);
            var secondSingleVoltageNode = replacableNode.CreateSingleVoltageNode(1,
                new HashSet<IExternalReadOnlyNode>(), false);

            var firstAsPq = firstSingleVoltageNode as PqNode;
            var secondAsPq = secondSingleVoltageNode as PqNode;
            var firstAsSlack = firstSingleVoltageNode as SlackNode;
            var secondAsSlack = secondSingleVoltageNode as SlackNode;
            var secondId = replacableNode.Id;

            if ((firstAsPq != null || firstAsSlack != null) && secondAsPq != null)
            {
                nodeResultsWithId.Add(secondId, new NodeResult(nodeResultUnscaled.Voltage, secondAsPq.Power));
                nodeResultsWithId.Add(id,
                    new NodeResult(nodeResultUnscaled.Voltage, nodeResultUnscaled.Power - secondAsPq.Power));
            }
            else if (firstAsPq != null && secondAsSlack != null)
            {
                nodeResultsWithId.Add(id, new NodeResult(nodeResultUnscaled.Voltage, firstAsPq.Power));
                nodeResultsWithId.Add(secondId,
                    new NodeResult(nodeResultUnscaled.Voltage, nodeResultUnscaled.Power - firstAsPq.Power));
            }
            else
                throw new NotSupportedException(
                    "the direct connections of these two node types is not yet supported");
        }

        public static SingleVoltageLevel.IPowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyList<Complex> constantCurrents, IPowerNetFactory singleVoltagePowerNetFactory)
        {
            var singleVoltagePowerNet = singleVoltagePowerNetFactory.Create(admittances.SingleVoltageAdmittanceMatrix, 1, constantCurrents);

            foreach (var node in nodes)
                singleVoltagePowerNet.AddNode(node.CreateSingleVoltageNode(scaleBasePower, new HashSet<IExternalReadOnlyNode>(), true));

            return singleVoltagePowerNet;
        }
    }
}
