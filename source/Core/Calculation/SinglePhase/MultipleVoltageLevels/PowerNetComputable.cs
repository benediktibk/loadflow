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
            int nodeCount;
            Dictionary<IReadOnlyNode, int> nodeIndices;
            IReadOnlyList<IReadOnlyNode> mainNodes;
            DetermineNodeIndices(nodes, out nodeCount, out nodeIndices, out mainNodes);
            var admittances = CalculateAdmittanceMatrix(nodeIndices, powerScaling, nodeCount);
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
            int nodeCount;
            Dictionary<IReadOnlyNode, int> nodeIndices;
            IReadOnlyList<IReadOnlyNode> mainNodes;
            DetermineNodeIndices(nodes, out nodeCount, out nodeIndices, out mainNodes);
            matrix = CalculateAdmittanceMatrix(nodeIndices, powerScaling, nodeCount);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private void DetermineNodeIndices(IReadOnlyList<IReadOnlyNode> nodes, out int count, out Dictionary<IReadOnlyNode, int> indices, out IReadOnlyList<IReadOnlyNode> mainNodes)
        {
            var directConnectedNodes = FindDirectConnectedNodes();
            mainNodes = nodes.Where(x => !directConnectedNodes.ContainsKey(x)).ToList();
            var replacableNodes = nodes.Where(directConnectedNodes.ContainsKey);
            count = mainNodes.Count;

            indices = new Dictionary<IReadOnlyNode, int>();

            for (var i = 0; i < mainNodes.Count; ++i)
            {
                var node = mainNodes[i];
                indices.Add(node, i);
            }

            foreach (var replacableNode in replacableNodes)
                indices.Add(replacableNode, indices[directConnectedNodes[replacableNode]]);
        }

        private SingleVoltageLevel.IPowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IAdmittanceMatrix admittances, double scaleBasePower)
        {
            var singleVoltagePowerNet = _singleVoltagePowerNetFactory.Create(admittances.SingleVoltageAdmittanceMatrix, 1);

            foreach (var node in nodes)
                singleVoltagePowerNet.AddNode(node.CreateSingleVoltageNode(scaleBasePower, new HashSet<IExternalReadOnlyNode>()));

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
