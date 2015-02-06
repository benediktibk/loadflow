using System.Collections.Generic;
using System.IO;
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

            var partialPowerNet = new PartialPowerNet(ExternalNodes, Elements, IdGeneratorNodes, AverageLoadFlow, _singleVoltagePowerNetFactory);
            return partialPowerNet.CalculateNodeResults(out relativePowerError);
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling)
        {
            var partialPowerNet = new PartialPowerNet(ExternalNodes, Elements, IdGeneratorNodes, AverageLoadFlow, _singleVoltagePowerNetFactory);
            partialPowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerScaling);
        }
    }
}
