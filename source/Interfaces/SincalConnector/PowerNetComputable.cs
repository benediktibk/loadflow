using System.Collections.Generic;
using System.Linq;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class PowerNetComputable : PowerNet
    {
        private SymmetricPowerNet _symmetricPowerNet;
        private readonly object _symmetricPowerNetMutex;
        private readonly double _powerFactor;

        public PowerNetComputable(double powerFactor)
        {
            _symmetricPowerNet = null;
            _symmetricPowerNetMutex = new object();
            _powerFactor = powerFactor;
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

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames,
            out double powerBase)
        {
            _symmetricPowerNet = CreateSymmetricPowerNet(null);
            _symmetricPowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);
        }

        private SymmetricPowerNet CreateSymmetricPowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var symmetricPowerNet = SymmetricPowerNet.Create(nodeVoltageCalculator, Frequency);

            foreach (var node in Nodes)
                node.AddTo(symmetricPowerNet);

            foreach (var element in NetElements)
                element.AddTo(symmetricPowerNet, _powerFactor);

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
