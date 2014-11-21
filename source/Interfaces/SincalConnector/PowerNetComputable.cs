using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class PowerNetComputable : PowerNet
    {
        public IReadOnlyDictionary<long, Calculation.NodeResult> CalculateNodeVoltages(INodeVoltageCalculator calculator, out Angle slackPhaseShift, out IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds, out double relativePowerError)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet(calculator);

            if (symmetricPowerNet.NodeGraph.FloatingNodesExist)
            {
                var floatingNodes = symmetricPowerNet.NodeGraph.FloatingNodes;
                var floatingNodeNames = floatingNodes.Select(x => x.Name);
                var nodeNamesCombined = floatingNodeNames.Aggregate((current, next) => current + ", " + next);
                throw new InvalidDataException("there are floating nodes: " + nodeNamesCombined);
            }

            var nominalPhaseShifts = symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            slackPhaseShift = ContainsTransformers ? symmetricPowerNet.SlackPhaseShift : new Angle();
            nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            return symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);
        }

        private SymmetricPowerNet CreateSymmetricPowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var singlePhasePowerNet = new Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable(Frequency, new PowerNetFactory(nodeVoltageCalculator), new NodeGraph());
            var symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);

            foreach (var node in Nodes)
                node.AddTo(symmetricPowerNet);

            foreach (var element in NetElements)
                element.AddTo(symmetricPowerNet);

            return symmetricPowerNet;
        }
    }
}
