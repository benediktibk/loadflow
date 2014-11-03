using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNet
    {
        private readonly double _frequency;
        private readonly List<Load> _loads;
        private readonly List<ImpedanceLoad> _impedanceLoads;
        private readonly List<TransmissionLine> _transmissionLines;
        private readonly List<TwoWindingTransformer> _twoWindingTransformers;
        private readonly List<ThreeWindingTransformer> _threeWindingTransformers;
        private readonly List<Generator> _generators;
        private readonly List<FeedIn> _feedIns;
        private readonly List<IPowerNetElement> _elements;
        private readonly List<Node> _nodes;
        private readonly Dictionary<long, Node> _nodesById;
        private readonly Node _groundNode;
        private readonly FeedIn _groundFeedIn;
        private readonly IdGenerator _idGeneratorNodes;
        private readonly INodeGraph _nodeGraph;

        public PowerNet(double frequency)
        {
            _frequency = frequency;
            _loads = new List<Load>();
            _impedanceLoads = new List<ImpedanceLoad>();
            _transmissionLines = new List<TransmissionLine>();
            _twoWindingTransformers = new List<TwoWindingTransformer>();
            _threeWindingTransformers = new List<ThreeWindingTransformer>();
            _generators = new List<Generator>();
            _feedIns = new List<FeedIn>();
            _elements = new List<IPowerNetElement>();
            _nodes = new List<Node>();
            _nodesById = new Dictionary<long, Node>();
            _idGeneratorNodes = new IdGenerator();
            _groundNode = new Node(_idGeneratorNodes.Generate(), 0, 0, "ground");
            _groundFeedIn = new FeedIn(_groundNode, new Complex(0, 0), 0, 1.1, 1, _idGeneratorNodes);
            _groundNode.Connect(_groundFeedIn);
            _nodesById.Add(_groundNode.Id, _groundNode);
            _nodeGraph = new NodeGraph();
        }

        public IReadOnlyNodeGraph NodeGraph
        {
            get { return _nodeGraph; }
        }

        public IReadOnlyList<IExternalReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IExternalReadOnlyNode>().ToList(); }
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

        public int NodeCount
        {
            get { return _loads.Count; }
        }

        public IReadOnlyNode GroundNode
        {
            get { return _groundNode; }
        }

        public Angle GetSlackPhaseShift()
        {
            if (_feedIns.Count != 1)
                throw new InvalidOperationException("there must exist exactly one feed in");

            var feedIn = _feedIns.First();
            return new Angle(feedIn.Voltage.Phase);
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> GetNominalPhaseShiftPerNode()
        {
            if (_feedIns.Count != 1)
                throw new InvalidOperationException("there must exist exactly one feed in");

            var feedIn = _feedIns.First();
            return _nodeGraph.CalculateNominalPhaseShiftPerNode(feedIn, _twoWindingTransformers, _threeWindingTransformers);
        }

        public IExternalReadOnlyNode GetNodeById(long id)
        {
            return GetNodeByIdInternal(id);
        }

        public void AddNode(int id, double nominalVoltage, double nominalPhaseShift, string name)
        {
            _idGeneratorNodes.Add(id);
            var node = new Node(id, nominalVoltage, nominalPhaseShift, name);
            _nodes.Add(node);
            _nodesById.Add(id, node);
            _nodeGraph.Add(node);
        }

        public void AddTransmissionLine(long sourceNodeId, long targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel)
        {
            var sourceNode = GetNodeByIdInternal(sourceNodeId);
            var targetNode = GetNodeByIdInternal(targetNodeId);
            var line = new TransmissionLine(sourceNode, targetNode, seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntCapacityPerUnitLength, shuntConductancePerUnitLength, length, _frequency, transmissionEquationModel);
            _transmissionLines.Add(line);
            _elements.Add(line);
            sourceNode.Connect(line);
            targetNode.Connect(line);
        }

        public void AddGenerator(long nodeId, double voltageMagnitude, double realPower)
        {
            var node = GetNodeByIdInternal(nodeId);
            var generator = new Generator(node, voltageMagnitude, realPower);
            _generators.Add(generator);
            _elements.Add(generator);
            node.Connect(generator);
        }

        public void AddFeedIn(long nodeId, Complex voltage, double shortCircuitPower, double c, double realToImaginary)
        {
            if (_feedIns.Count > 0)
                throw new NotSupportedException("only one slack bus is supported");

            var node = GetNodeByIdInternal(nodeId);
            var feedIn = new FeedIn(node, voltage, shortCircuitPower, c, realToImaginary, _idGeneratorNodes);
            _feedIns.Add(feedIn);
            _elements.Add(feedIn);
            node.Connect(feedIn);
        }

        public void AddTwoWindingTransformer(long upperSideNodeId, long lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name)
        {
            var upperSideNode = GetNodeByIdInternal(upperSideNodeId);
            var lowerSideNode = GetNodeByIdInternal(lowerSideNodeId);
            var transformer = new TwoWindingTransformer(upperSideNode, lowerSideNode, nominalPower, relativeShortCircuitVoltage, copperLosses, ironLosses, relativeNoLoadCurrent, ratio, nominalPhaseShift, name, _idGeneratorNodes);
            _twoWindingTransformers.Add(transformer);
            _elements.Add(transformer);
            upperSideNode.Connect(transformer);
            lowerSideNode.Connect(transformer);
        }

        public void AddThreeWindingTransformer(long nodeOneId, long nodeTwoId, long nodeThreeId, double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree, double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo, 
            double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses, double relativeNoLoadCurrent, Angle nominalPhaseShiftOneToTwo, 
            Angle nominalPhaseShiftTwoToThree, Angle nominalPhaseShiftThreeToOne, string name)
        {
            var nodeOne = GetNodeByIdInternal(nodeOneId);
            var nodeTwo = GetNodeByIdInternal(nodeTwoId);
            var nodeThree = GetNodeByIdInternal(nodeThreeId);
            var transformer = new ThreeWindingTransformer(nodeOne, nodeTwo, nodeThree, nominalPowerOneToTwo, nominalPowerTwoToThree,
                nominalPowerThreeToOne, relativeShortCircuitVoltageOneToTwo, relativeShortCircuitVoltageTwoToThree,
                relativeShortCircuitVoltageThreeToOne, copperLossesOneToTwo, copperLossesTwoToThree, copperLossesThreeToOne,
                ironLosses, relativeNoLoadCurrent, nominalPhaseShiftOneToTwo, nominalPhaseShiftTwoToThree, nominalPhaseShiftThreeToOne,
                name, _idGeneratorNodes);
            _threeWindingTransformers.Add(transformer);
            _elements.Add(transformer);
            nodeOne.Connect(transformer);
            nodeTwo.Connect(transformer);
            nodeThree.Connect(transformer);
        }

        public void AddLoad(long nodeId, Complex power)
        {
            var node = GetNodeByIdInternal(nodeId);
            var load = new Load(power, node);
            _loads.Add(load);
            _elements.Add(load);
            node.Connect(load);
        }

        public void AddImpedanceLoad(long nodeId, Complex impedance)
        {
            var node = GetNodeByIdInternal(nodeId);
            var impedanceLoad = new ImpedanceLoad(node, impedance);
            _impedanceLoads.Add(impedanceLoad);
            _elements.Add(impedanceLoad);
            node.Connect(impedanceLoad);
        }

        public bool CheckIfNominalVoltagesDoNotMatch()
        {
            return _elements.Exists(element => !element.NominalVoltagesMatch);
        }

        public bool CheckIfNodeIsOverdetermined()
        {
            return _nodes.Count(x => x.IsOverdetermined) > 0;
        }

        public bool IsGroundNodeNecessary()
        {
            return _elements.Exists(x => x.NeedsGroundNode);
        }

        public IReadOnlyList<IReadOnlyNode> GetAllNecessaryNodes()
        {
            var allNodes = new List<IReadOnlyNode>();

            foreach (var element in _elements)
                allNodes.AddRange(element.GetInternalNodes());
            allNodes.AddRange(_nodes);

            if (!IsGroundNodeNecessary()) 
                return allNodes;

            allNodes.AddRange(_groundFeedIn.GetInternalNodes());
            allNodes.Add(_groundNode);

            return allNodes;
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasePower)
        {
            var expectedLoadFlow = CalculateAverageLoadFlow();
            foreach (var element in _elements)
                element.FillInAdmittances(admittances, scaleBasePower, _groundNode, expectedLoadFlow);
        }

        private Node GetNodeByIdInternal(long name)
        {
            Node result;
            _nodesById.TryGetValue(name, out result);

            if (result == default(Node))
                throw new ArgumentOutOfRangeException("name", "specified node does not exist");

            return result;
        }

        public double CalculateAverageLoadFlow()
        {
            var absoluteSum = 
                _generators.Sum(generator => Math.Abs(generator.RealPower)) + 
                _loads.Sum(load => Math.Abs(load.Value.Real));
            var count = _generators.Count + _loads.Count;

            if (count == 0)
                return 0;

            return absoluteSum/count;
        }

        public double GetMaximumPower()
        {
            var generatorMaximum = _generators.Count > 0 ? _generators.Max(generator => Math.Abs(generator.RealPower)) : 0;
            var loadMaximum = _loads.Count > 0 ? _loads.Max(load => Math.Abs(load.Value.Real)) : 0;
            var transformerMaximum = _twoWindingTransformers.Count > 0
                ? _twoWindingTransformers.Max(transformer => transformer.NominalPower)
                : 0;
            return Math.Max(Math.Max(generatorMaximum, loadMaximum), transformerMaximum);
        }

        public double DeterminePowerScaling()
        {
            var maximumPower = GetMaximumPower();
            var powerScaling = maximumPower > 0 ? maximumPower : 1;
            return powerScaling;
        }
    }
}
