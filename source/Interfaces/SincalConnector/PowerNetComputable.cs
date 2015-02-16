using System.Collections.Generic;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class PowerNetComputable : PowerNet
    {
        private SymmetricPowerNet _symmetricPowerNet;
        private readonly object _symmetricPowerNetMutex;

        public PowerNetComputable()
        {
            _symmetricPowerNet = null;
            _symmetricPowerNetMutex = new object();
        }

        public IReadOnlyDictionary<int, Calculation.NodeResult> CalculateNodeVoltages(INodeVoltageCalculator calculator, out Angle slackPhaseShift, out IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds, out double relativePowerError)
        {
            lock (_symmetricPowerNetMutex)
            {
                _symmetricPowerNet = CreateSymmetricPowerNet(calculator);
            }

            var nominalPhaseShifts = _symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            slackPhaseShift = ContainsTransformers && CountOfElementsWithSlackBus == 1 ? _symmetricPowerNet.SlackPhaseShift : new Angle();
            nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            return _symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);
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

        public double TotalProgress
        {
            get
            {
                lock (_symmetricPowerNetMutex)
                {
                    return _symmetricPowerNet == null ? 0 : _symmetricPowerNet.TotalProgress;
                }
            }
        }
    }
}
