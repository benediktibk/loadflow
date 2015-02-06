using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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
            var mainNodes = PartialPowerNet.SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = PartialPowerNet.SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = PartialPowerNet.DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            var admittances = CalculateAdmittanceMatrix(nodeIndices, powerScaling, mainNodes.Count);
            var constantCurrents = CalculateConstantCurrents(mainNodes, powerScaling, nodeIndices);
            var singleVoltagePowerNet = PartialPowerNet.CreateSingleVoltagePowerNet(mainNodes, admittances, powerScaling, constantCurrents, _singleVoltagePowerNetFactory);
            var nodeResults = singleVoltagePowerNet.CalculateNodeResults(out relativePowerError);
            return nodeResults == null ? null : PartialPowerNet.CreateNodeResultsWithId(directConnectedNodes, replacableNodes, nodeIndices, nodeResults, powerScaling, ExternalNodes);
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling)
        {
            powerScaling = DeterminePowerScaling();
            var nodes = GetAllCalculationNodes();
            var directConnectedNodes = FindDirectConnectedNodes();
            var mainNodes = PartialPowerNet.SelectMainNodes(nodes, directConnectedNodes);
            var replacableNodes = PartialPowerNet.SelectReplacableNodes(nodes, directConnectedNodes);
            var nodeIndices = PartialPowerNet.DetermineNodeIndices(directConnectedNodes, mainNodes, replacableNodes);
            matrix = CalculateAdmittanceMatrix(nodeIndices, powerScaling, mainNodes.Count);
            nodeNames = nodes.Select(node => node.Name).ToList();
        }

        private AdmittanceMatrix CalculateAdmittanceMatrix(IReadOnlyDictionary<IReadOnlyNode, int> nodeIndices, double scaleBasePower, int nodeCount)
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
    }
}
