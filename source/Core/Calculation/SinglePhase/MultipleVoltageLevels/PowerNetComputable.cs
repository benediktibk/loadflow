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
            var directConnectionsReverse = directConnectedNodes.ToDictionary(connection => connection.Value, connection => connection.Key);
            var replacableNodesSet = new HashSet<IReadOnlyNode>();

            foreach (var replacableNode in replacableNodes)
                replacableNodesSet.Add(replacableNode);


            var externalMainNodes = ExternalNodes.Where(x => !replacableNodesSet.Contains(x));
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
                        nodeResultsWithId.Add(id, new NodeResult(nodeResultUnscaled.Voltage, nodeResultUnscaled.Power - secondAsPq.Power));
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

        private static IReadOnlyList<IReadOnlyNode> SelectReplacableNodes(IEnumerable<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes)
        {
            return nodes.Where(directConnectedNodes.ContainsKey).ToList();
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
