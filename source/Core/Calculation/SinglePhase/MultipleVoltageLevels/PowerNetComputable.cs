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
            var nodeIndexes = DetermineNodeIndices(nodes);
            var admittances = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerScaling);
            var singleVoltagePowerNet = CreateSingleVoltagePowerNet(nodes, admittances, powerScaling);
            var nodeResults = singleVoltagePowerNet.CalculateNodeResults(out relativePowerError);

            if (nodeResults == null)
                return null;

            var nodeResultsWithId = new Dictionary<int, NodeResult>();

            foreach (var node in ExternalNodes)
            {
                var nodeIndex = nodeIndexes[node];
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
            var nodeIndexes = DetermineNodeIndices(nodes);
            matrix = CalculateAdmittanceMatrix(nodes, nodeIndexes, powerScaling);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private Dictionary<IReadOnlyNode, int> DetermineNodeIndices(IReadOnlyList<IReadOnlyNode> nodes)
        {
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = nodes.Where(x => !directConnectedNodes.ContainsKey(x)).ToList();
            var replacableNodes = nodes.Where(directConnectedNodes.ContainsKey);
            
            var nodeIndices = new Dictionary<IReadOnlyNode, int>();

            for (var i = 0; i < mainNodes.Count; ++i)
            {
                var node = mainNodes[i];
                nodeIndices.Add(node, i);
            }

            foreach (var replacableNode in replacableNodes)
                nodeIndices.Add(replacableNode, nodeIndices[directConnectedNodes[replacableNode]]);

            return nodeIndices;
        }

        private SingleVoltageLevel.IPowerNetComputable CreateSingleVoltagePowerNet(IEnumerable<IReadOnlyNode> nodes, IAdmittanceMatrix admittances, double scaleBasePower)
        {
            var singleVoltagePowerNet = _singleVoltagePowerNetFactory.Create(admittances.SingleVoltageAdmittanceMatrix, 1);

            foreach (var node in nodes)
                singleVoltagePowerNet.AddNode(node.CreateSingleVoltageNode(scaleBasePower));

            return singleVoltagePowerNet;
        }

        private AdmittanceMatrix CalculateAdmittanceMatrix(IReadOnlyCollection<IReadOnlyNode> nodes, IReadOnlyDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasePower)
        {
            var admittances = new AdmittanceMatrix(new SingleVoltageLevel.AdmittanceMatrix(nodes.Count), nodeIndexes);
            FillInAdmittances(admittances, scaleBasePower);
            return admittances;
        }
    }
}
