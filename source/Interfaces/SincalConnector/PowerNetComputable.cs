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
        public IReadOnlyDictionary<long, Calculation.NodeResult> CalculateNodeVoltages(INodeVoltageCalculator calculator, out Angle slackPhaseShift, 
            out IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet(calculator);

            if (symmetricPowerNet.NodeGraph.FloatingNodesExist)
                throw new InvalidDataException("there must not be a floating node");

            var nominalPhaseShifts = symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            slackPhaseShift = ContainsTransformers ? symmetricPowerNet.SlackPhaseShift : new Angle();
            nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            return symmetricPowerNet.CalculateNodeVoltages();
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
