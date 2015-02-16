using System.Collections.Generic;
using System.IO;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNetComputable : PowerNet, IPowerNetComputable
    {
        private readonly IPowerNetFactory _singleVoltagePowerNetFactory;
        private readonly object _progressMutex;
        private double _progress;

        public PowerNetComputable(double frequency, IPowerNetFactory singleVoltagePowerNetFactory, INodeGraph nodeGraph) : base(frequency, nodeGraph)
        {
            _singleVoltagePowerNetFactory = singleVoltagePowerNetFactory;
            _progressMutex = new object();
            _progress = 0;
        }

        public double TotalProgress
        {
            get
            {
                lock (_progressMutex)
                {
                    return _progress;
                }
            }

            private set
            {
                lock (_progressMutex)
                {
                    _progress = value;
                }
            }
        }

        public IReadOnlyDictionary<int, NodeResult> CalculateNodeResults(out double relativePowerError)
        {
            TotalProgress = 0;

            if (NominalVoltagesDoNotMatch)
                throw new InvalidDataException("the nominal voltages must match on connected nodes");

            relativePowerError = 0;
            var result = new Dictionary<int, NodeResult>();
            var partialPowerNets =
                NodeGraph.Segments.Select(
                    x =>
                        new PartialPowerNet(x.ToList(), FindElementsOfSegment(x), IdGeneratorNodes, AverageLoadFlow,
                            _singleVoltagePowerNetFactory)).OrderBy(x => x.NodeCount).ToList();
            var totalNodeCount = partialPowerNets.Sum(x => x.NodeCount);
            var processedNodeCount = 0;

            foreach (var partialPowerNet in partialPowerNets)
            {
                double partialRelativePowerError;
                var partialResult = partialPowerNet.CalculateNodeResults(out partialRelativePowerError);
                relativePowerError += partialRelativePowerError;

                if (partialResult == null)
                    return null;

                foreach (var pair in partialResult)
                    result.Add(pair.Key, pair.Value);

                processedNodeCount += partialPowerNet.NodeCount;
                TotalProgress = (double)processedNodeCount / totalNodeCount;
            }

            return result;
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerScaling)
        {
            var partialPowerNet = new PartialPowerNet(ExternalNodes, Elements, IdGeneratorNodes, AverageLoadFlow, _singleVoltagePowerNetFactory);
            partialPowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerScaling);
        }

        private List<IPowerNetElement> FindElementsOfSegment(ISet<IExternalReadOnlyNode> nodes)
        {
            return Elements.Where(x => x.IsConnectedTo(nodes)).ToList();
        }
    }
}
