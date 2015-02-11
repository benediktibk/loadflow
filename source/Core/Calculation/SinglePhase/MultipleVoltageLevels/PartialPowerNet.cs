using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    class PartialPowerNet
    {
        private readonly IReadOnlyList<IExternalReadOnlyNode> _nodes;
        private readonly IReadOnlyList<IPowerNetElement> _elements;
        private readonly ExternalNode _groundNode;
        private readonly FeedIn _groundFeedIn;
        private readonly double _averageLoadFlow;
        private readonly IPowerNetFactory _singleVoltagePowerNetFactory;

        public PartialPowerNet(IReadOnlyList<IExternalReadOnlyNode> nodes, IReadOnlyList<IPowerNetElement> elements, IdGenerator idGenerator, double averagleLoadFlow, IPowerNetFactory singleVoltagePowerNetFactory)
        {
            _nodes = nodes;
            _elements = elements;
            _groundNode = new ExternalNode(idGenerator.Generate(), 0, "ground");
            _groundFeedIn = new FeedIn(_groundNode, new Complex(), new Complex(), idGenerator);
            _groundNode.Connect(_groundFeedIn);
            _averageLoadFlow = averagleLoadFlow;
            _singleVoltagePowerNetFactory = singleVoltagePowerNetFactory;
        }

        public IReadOnlyDictionary<int, NodeResult> CalculateNodeResults(out double relativePowerError)
        {
            var powerScaling = DeterminePowerScaling();
            var nodes = GetAllCalculationNodes();
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            var admittances = CalculateAdmittanceMatrixInternal(nodeIndices, powerScaling, mainNodes.Count);
            var constantCurrents = CalculateConstantCurrents(mainNodes, powerScaling, nodeIndices);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(mainNodes, admittances, powerScaling, constantCurrents);
            var nodeResults = singleVoltagePowerNet.CalculateNodeResults(out relativePowerError);
            return nodeResults == null ? null : CreateNodeResultsWithId(directConnectedNodes, replacableNodes, nodeIndices, nodeResults, powerScaling, _nodes);
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling)
        {
            powerScaling = DeterminePowerScaling();
            var nodes = GetAllCalculationNodes();
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            matrix = CalculateAdmittanceMatrixInternal(nodeIndices, powerScaling, mainNodes.Count);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private static Dictionary<IReadOnlyNode, int> DetermineNodeIndices(IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> directConnectedNodes, IReadOnlyList<IReadOnlyNode> mainNodes, IEnumerable<IReadOnlyNode> replacableNodes)
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

        private static Dictionary<int, NodeResult> CreateNodeResultsWithId(IEnumerable<KeyValuePair<IReadOnlyNode, IReadOnlyNode>> directConnectedNodes, IEnumerable<IReadOnlyNode> replacableNodes,
            IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, IList<NodeResult> nodeResults, double powerScaling, IEnumerable<IExternalReadOnlyNode> externalNodes)
        {
            var nodeResultsWithId = new Dictionary<int, NodeResult>();
            var replacableNodesSet = new HashSet<IReadOnlyNode>();
            var directConnectionsReverse = new MultiDictionary<IReadOnlyNode, IReadOnlyNode>();

            foreach (var directConnectedNode in directConnectedNodes)
                directConnectionsReverse.Add(directConnectedNode.Value, directConnectedNode.Key);

            foreach (var replacableNode in replacableNodes)
                replacableNodesSet.Add(replacableNode);

            var externalMainNodes = externalNodes.Where(x => !replacableNodesSet.Contains(x));
            foreach (var node in externalMainNodes)
            {
                var nodeIndex = nodeIndices[node];
                var nodeResult = nodeResults[nodeIndex];
                var id = node.Id;
                var nodeResultUnscaled = nodeResult.Unscale(node.NominalVoltage, powerScaling);
                var replacedNodes = directConnectionsReverse.Get(node);

                if (replacedNodes.Count == 0)
                    nodeResultsWithId.Add(id, nodeResultUnscaled);
                else
                    AddNodeResultWithIdForDirectConnectedNodes(nodeResultsWithId, node, replacedNodes, id, nodeResultUnscaled);
            }

            return nodeResultsWithId;
        }

        private static void AddNodeResultWithIdForDirectConnectedNodes(IDictionary<int, NodeResult> nodeResultsWithId, IReadOnlyNode node, IReadOnlyList<IReadOnlyNode> replacedNodes, int id, NodeResult nodeResult)
        {
            var allNodes = new List<IReadOnlyNode> {node};
            allNodes.AddRange(replacedNodes);
            var singleVoltageNodes = new List<Tuple<INode, int>>(allNodes.Count);
            singleVoltageNodes.AddRange(
                replacedNodes.Select(
                    x => new Tuple<INode, int>(x.CreateSingleVoltageNode(1, new HashSet<IExternalReadOnlyNode>(), false), x.Id)));
            singleVoltageNodes.Add(
                new Tuple<INode, int>(node.CreateSingleVoltageNode(1, new HashSet<IExternalReadOnlyNode>(), false), id));

            var slackNodes =
                singleVoltageNodes.Select(x => new Tuple<SlackNode, int>(x.Item1 as SlackNode, x.Item2))
                    .Where(x => x.Item1 != null)
                    .ToList();
            var pqNodes =
                singleVoltageNodes.Select(x => new Tuple<PqNode, int>(x.Item1 as PqNode, x.Item2))
                    .Where(x => x.Item1 != null)
                    .ToList();

            if (slackNodes.Count() + pqNodes.Count() != singleVoltageNodes.Count)
                throw new NotSupportedException("can not separate the PV- from the other nodes");

            var voltage = nodeResult.Voltage;
            var power = nodeResult.Power;
            IReadOnlyList<int> leftOverIds;

            if (slackNodes.Any())
                leftOverIds = slackNodes.Select(x => x.Item2).ToList();
            else
            {
                leftOverIds = new List<int> {pqNodes.Last().Item2};
                pqNodes.RemoveAt(pqNodes.Count - 1);
            }

            foreach (var pqNode in pqNodes)
            {
                var ownPower = pqNode.Item1.Power;
                power = power - ownPower;
                nodeResultsWithId.Add(pqNode.Item2, new NodeResult(voltage, ownPower));
            }

            foreach (var leftOverId in leftOverIds)
                nodeResultsWithId.Add(leftOverId, new NodeResult(voltage, power/leftOverIds.Count));
        }

        private IReadOnlyDictionary<IReadOnlyNode, IReadOnlyNode> FindDirectConnectedNodes()
        {
            var allDirectConnectedNodes = new HashSet<IExternalReadOnlyNode>();
            var directConnectedNodes = new List<ISet<IExternalReadOnlyNode>>();
            var result = new Dictionary<IReadOnlyNode, IReadOnlyNode>();

            foreach (var node in _nodes)
            {
                if (allDirectConnectedNodes.Contains(node))
                    continue;

                var segment = new HashSet<IExternalReadOnlyNode>();
                node.AddDirectConnectedNodes(segment);

                if (segment.Count == 1)
                    continue;

                directConnectedNodes.Add(segment);

                foreach (var segmentNode in segment)
                    allDirectConnectedNodes.Add(segmentNode);
            }

            foreach (var segment in directConnectedNodes)
            {
                var mainElement = segment.First();

                foreach (var node in segment.Where(x => x != mainElement))
                    result.Add(node, mainElement);
            }

            return result;
        }

        private SingleVoltageLevel.IPowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IAdmittanceMatrix admittances, double scaleBasePower, IReadOnlyList<Complex> constantCurrents)
        {
            var singleVoltagePowerNet = _singleVoltagePowerNetFactory.Create(admittances.SingleVoltageAdmittanceMatrix, 1, constantCurrents);

            foreach (var node in nodes)
                singleVoltagePowerNet.AddNode(node.CreateSingleVoltageNode(scaleBasePower, new HashSet<IExternalReadOnlyNode>(), true));

            return singleVoltagePowerNet;
        }

        private double DeterminePowerScaling()
        {
            var maximumPower = _elements.Max(x => x.MaximumPower);
            var powerScaling = maximumPower > 0 ? maximumPower : 1;
            return powerScaling;
        }

        private IReadOnlyList<IReadOnlyNode> GetAllCalculationNodes()
        {
            var allNodes = new List<IReadOnlyNode>();

            foreach (var element in _elements)
                allNodes.AddRange(element.GetInternalNodes());
            allNodes.AddRange(_nodes);

            if (!_elements.Any(x => x.NeedsGroundNode))
                return allNodes;

            allNodes.AddRange(_groundFeedIn.GetInternalNodes());
            allNodes.Add(_groundNode);

            return allNodes;
        }

        private AdmittanceMatrix CalculateAdmittanceMatrixInternal(IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower, int nodeCount)
        {
            var admittances = new AdmittanceMatrix(new SingleVoltageLevel.AdmittanceMatrix(nodeCount), nodeIndices);
            FillInAdmittances(admittances, scaleBasePower);
            return admittances;
        }

        private IReadOnlyList<Complex> CalculateConstantCurrents(IReadOnlyCollection<IReadOnlyNode> mainNodes, double powerScaling, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices)
        {
            var constantCurrents = new Complex[mainNodes.Count];
            FillInConstantCurrents(constantCurrents, powerScaling, nodeIndices);
            return constantCurrents;
        }

        private void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower)
        {
            foreach (var element in _elements)
                element.FillInAdmittances(admittances, scaleBasePower, _groundNode, _averageLoadFlow);
        }

        private void FillInConstantCurrents(IList<Complex> currents, double scaleBasePower, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices)
        {
            foreach (var element in _elements)
                element.FillInConstantCurrents(currents, nodeIndices, scaleBasePower);
        }
    }
}
