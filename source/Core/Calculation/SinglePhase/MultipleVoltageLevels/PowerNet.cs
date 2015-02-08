using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNet : IPowerNet
    {
        private readonly double _frequency;
        private readonly List<Load> _loads;
        private readonly List<ImpedanceLoad> _impedanceLoads;
        private readonly List<TransmissionLine> _transmissionLines;
        private readonly List<TwoWindingTransformer> _twoWindingTransformers;
        private readonly List<ThreeWindingTransformer> _threeWindingTransformers;
        private readonly List<Generator> _generators;
        private readonly List<FeedIn> _feedIns;
        private readonly List<CurrentSource> _currentSources; 
        private readonly List<IPowerNetElement> _elements;
        private readonly List<ExternalNode> _nodes;
        private readonly Dictionary<long, ExternalNode> _nodesById;
        private readonly IdGenerator _idGeneratorNodes;
        private readonly INodeGraph _nodeGraph;

        public PowerNet(double frequency, INodeGraph nodeGraph)
        {
            _frequency = frequency;
            _loads = new List<Load>();
            _currentSources = new List<CurrentSource>();
            _impedanceLoads = new List<ImpedanceLoad>();
            _transmissionLines = new List<TransmissionLine>();
            _twoWindingTransformers = new List<TwoWindingTransformer>();
            _threeWindingTransformers = new List<ThreeWindingTransformer>();
            _generators = new List<Generator>();
            _feedIns = new List<FeedIn>();
            _elements = new List<IPowerNetElement>();
            _nodes = new List<ExternalNode>();
            _nodesById = new Dictionary<long, ExternalNode>();
            _idGeneratorNodes = new IdGenerator();
            _nodeGraph = nodeGraph;
        }

        public IdGenerator IdGeneratorNodes
        {
            get { return _idGeneratorNodes; }
        }

        public IReadOnlyNodeGraph NodeGraph
        {
            get { return _nodeGraph; }
        }

        public IReadOnlyList<IExternalReadOnlyNode> ExternalNodes
        {
            get { return _nodes.Cast<IExternalReadOnlyNode>().ToList(); }
        }

        public IReadOnlyList<IPowerNetElement> Elements
        {
             get { return _elements; }
        }

        public int LoadCount
        {
            get { return _loads.Count; }
        }

        public int ImpedanceLoadCount
        {
            get { return _impedanceLoads.Count; }
        }

        public int LineCount
        {
            get { return _transmissionLines.Count; }
        }

        public int FeedInCount
        {
            get { return _feedIns.Count; }
        }

        public int TwoWindingTransformerCount
        {
            get { return _twoWindingTransformers.Count; }
        }

        public int ThreeWindingTransformerCount
        {
            get { return _threeWindingTransformers.Count; }
        }

        public int GeneratorCount
        {
            get { return _generators.Count; }
        }

        public int CurrentSourceCount
        {
            get { return _currentSources.Count; }
        }

        public Angle SlackPhaseShift
        {
            get
            {
                if (_feedIns.Count != 1)
                    throw new InvalidOperationException("there must exist exactly one feed in");

                var feedIn = _feedIns.First();
                return new Angle(feedIn.Voltage.Phase);
            }
        }

        public double Frequency
        {
            get { return _frequency; }
        }

        public bool NominalVoltagesDoNotMatch
        {
            get { return _elements.Exists(element => !element.NominalVoltagesMatch); }
        }

        public double AverageLoadFlow
        {
            get
            {
                var absoluteSum =
                    _generators.Sum(generator => Math.Abs(generator.RealPower)) +
                    _loads.Sum(load => Math.Abs(load.Value.Real));
                var count = _generators.Count + _loads.Count;

                if (count == 0)
                    return 0;

                return absoluteSum / count;
            }
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> NominalPhaseShiftPerNode
        {
            get
            {
                if (_feedIns.Count == 0)
                    throw new InvalidOperationException("there must exist at least one feed in");

                var feedInNodes = _feedIns.Select(x => x.Node);
                return _nodeGraph.CalculateNominalPhaseShiftPerNode(feedInNodes, _twoWindingTransformers, _threeWindingTransformers);
            }
        }

        public IExternalReadOnlyNode GetNodeById(long id)
        {
            return GetNodeByIdInternal(id);
        }

        public void AddNode(int id, double nominalVoltage, string name)
        {
            _idGeneratorNodes.Add(id);
            var node = new ExternalNode(id, nominalVoltage, name);
            _nodes.Add(node);
            _nodesById.Add(id, node);
            _nodeGraph.Add(node);
        }

        public void AddTransmissionLine(int sourceNodeId, int targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel)
        {
            var sourceNode = GetNodeByIdInternal(sourceNodeId);
            var targetNode = GetNodeByIdInternal(targetNodeId);
            var line = new TransmissionLine(sourceNode, targetNode, seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength, length, _frequency, transmissionEquationModel);
            _transmissionLines.Add(line);
            _elements.Add(line);
            sourceNode.Connect(line);
            targetNode.Connect(line);
        }

        public void AddGenerator(int nodeId, double voltageMagnitude, double realPower)
        {
            var node = GetNodeByIdInternal(nodeId);
            var generator = new Generator(node, voltageMagnitude, realPower);
            _generators.Add(generator);
            _elements.Add(generator);
            node.Connect(generator);
        }

        public void AddFeedIn(int nodeId, Complex voltage, Complex internalImpedance)
        {
            var node = GetNodeByIdInternal(nodeId);
            var feedIn = new FeedIn(node, voltage, internalImpedance, _idGeneratorNodes);
            _feedIns.Add(feedIn);
            _elements.Add(feedIn);
            node.Connect(feedIn);
        }

        public void AddTwoWindingTransformer(int upperSideNodeId, int lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name)
        {
            var upperSideNode = GetNodeByIdInternal(upperSideNodeId);
            var lowerSideNode = GetNodeByIdInternal(lowerSideNodeId);
            var transformer = new TwoWindingTransformer(upperSideNode, lowerSideNode, nominalPower, relativeShortCircuitVoltage, copperLosses, ironLosses, relativeNoLoadCurrent, ratio, nominalPhaseShift, name, _idGeneratorNodes);
            _twoWindingTransformers.Add(transformer);
            _elements.Add(transformer);
            upperSideNode.Connect(transformer);
            lowerSideNode.Connect(transformer);
        }

        public void AddThreeWindingTransformer(int nodeOneId, int nodeTwoId, int nodeThreeId, double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne, double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree, double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses, double relativeNoLoadCurrent, Angle nominalPhaseShiftOne, Angle nominalPhaseShiftTwo, Angle nominalPhaseShiftThree, string name)
        {
            var nodeOne = GetNodeByIdInternal(nodeOneId);
            var nodeTwo = GetNodeByIdInternal(nodeTwoId);
            var nodeThree = GetNodeByIdInternal(nodeThreeId);
            var transformer = new ThreeWindingTransformer(nodeOne, nodeTwo, nodeThree, nominalPowerOneToTwo, nominalPowerTwoToThree,
                nominalPowerThreeToOne, relativeShortCircuitVoltageOneToTwo, relativeShortCircuitVoltageTwoToThree,
                relativeShortCircuitVoltageThreeToOne, copperLossesOneToTwo, copperLossesTwoToThree, copperLossesThreeToOne,
                ironLosses, relativeNoLoadCurrent, nominalPhaseShiftOne, nominalPhaseShiftTwo, nominalPhaseShiftThree,
                name, _idGeneratorNodes);
            _threeWindingTransformers.Add(transformer);
            _elements.Add(transformer);
            nodeOne.Connect(transformer);
            nodeTwo.Connect(transformer);
            nodeThree.Connect(transformer);
        }

        public void AddLoad(int nodeId, Complex power)
        {
            var node = GetNodeByIdInternal(nodeId);
            var load = new Load(power, node);
            _loads.Add(load);
            _elements.Add(load);
            node.Connect(load);
        }

        public void AddImpedanceLoad(int nodeId, Complex impedance)
        {
            var node = GetNodeByIdInternal(nodeId);
            var impedanceLoad = new ImpedanceLoad(node, impedance);
            _impedanceLoads.Add(impedanceLoad);
            _elements.Add(impedanceLoad);
            node.Connect(impedanceLoad);
        }

        public void AddCurrentSource(int nodeId, Complex current, Complex internalImpedance)
        {
            var node = GetNodeByIdInternal(nodeId);
            var currentSource = new CurrentSource(node, current, internalImpedance, _idGeneratorNodes);
            _currentSources.Add(currentSource);
            _elements.Add(currentSource);
            node.Connect(currentSource);
        }

        private ExternalNode GetNodeByIdInternal(long name)
        {
            ExternalNode result;
            _nodesById.TryGetValue(name, out result);

            if (result == default(ExternalNode))
                throw new ArgumentOutOfRangeException("name", "specified node does not exist");

            return result;
        }
    }
}
