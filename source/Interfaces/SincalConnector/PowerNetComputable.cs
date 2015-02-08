using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class PowerNetComputable : PowerNet
    {
        public IReadOnlyDictionary<int, Calculation.NodeResult> CalculateNodeVoltages(INodeVoltageCalculator calculator, out Angle slackPhaseShift, out IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds, out double relativePowerError)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet(calculator);
            var nominalPhaseShifts = symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            slackPhaseShift = ContainsTransformers && CountOfElementsWithSlackBus == 1 ? symmetricPowerNet.SlackPhaseShift : new Angle();
            nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            return symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);
        }

        private SymmetricPowerNet CreateSymmetricPowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var symmetricPowerNet = SymmetricPowerNet.Create(nodeVoltageCalculator, Frequency);

            foreach (var node in Nodes)
                node.AddTo(symmetricPowerNet);

            foreach (var element in NetElements)
                element.AddTo(symmetricPowerNet, 1);

            return symmetricPowerNet;
        }
    }
}
