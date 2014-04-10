using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class PowerNet
    {
        #region variables

        private readonly double _frequency;
        private readonly IList<Load> _loads;
        private readonly IList<Line> _lines;
        private readonly IList<Transformator> _transformators;
        private readonly IList<Generator> _generators;
        private readonly IList<FeedIn> _feedIns; 
        private readonly IList<Node> _nodes;
        private readonly IDictionary<string, Node> _nodesByName; 

        #endregion

        #region constructor

        public PowerNet(double frequency)
        {
            _frequency = frequency;
            _loads = new List<Load>();
            _lines = new List<Line>();
            _transformators = new List<Transformator>();
            _generators = new List<Generator>();
            _feedIns = new List<FeedIn>();
            _nodes = new List<Node>();
            _nodesByName = new Dictionary<string, Node>();
        }

        #endregion

        #region properties

        public int LoadCount
        {
            get { return _loads.Count; }
        }

        public int LineCount
        {
            get { return _lines.Count; }
        }

        public int FeedInCount
        {
            get { return _feedIns.Count; }
        }

        public int TransformatorCount
        {
            get { return _transformators.Count; }
        }

        public int GeneratorCount
        {
            get { return _generators.Count; }
        }

        public int NodeCount
        {
            get { return _loads.Count; }
        }

        #endregion

        #region public functions

        public void AddNode(string name, double nominalVoltage)
        {
            if (_nodesByName.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "a node with this name already exists");

            var node = new Node(name, nominalVoltage);
            _nodes.Add(node);
            _nodesByName.Add(name, node);
        }

        public void AddLine(string name, string sourceNodeName, string targetNodeName, double lengthResistance, double lengthInductance,
            double shuntConductance, double capacity)
        {
            var sourceNode = GetNodeByNameInternal(sourceNodeName);
            var targetNode = GetNodeByNameInternal(targetNodeName);
            var line = new Line(name, sourceNode, targetNode);
            _lines.Add(line);
            sourceNode.Connect(line);
            targetNode.Connect(line);
        }

        public void AddGenerator(string nodeName, string name, double synchronLengthInductance, double synchronousGeneratedVoltage)
        {
            var node = GetNodeByNameInternal(nodeName);
            var generator = new Generator(name, node);
            _generators.Add(generator);
            node.Connect(generator);
        }

        public void AddFeedIn(string nodeName, string name, Complex voltage)
        {
            var node = GetNodeByNameInternal(nodeName);
            var feedIn = new FeedIn(name, node, voltage);
            _feedIns.Add(feedIn);
            node.Connect(feedIn);
        }

        public void AddTransformator(string upperSideNodeName, string lowerSideNodeName, string name, double nominalPower,
            double shortCircuitVoltageInPercentage, double copperLosses, double ironLosses, double alpha)
        {
            var upperSideNode = GetNodeByNameInternal(upperSideNodeName);
            var lowerSideNode = GetNodeByNameInternal(lowerSideNodeName);
            var transformator = new Transformator(name, nominalPower, shortCircuitVoltageInPercentage, copperLosses, ironLosses, alpha, upperSideNode, lowerSideNode);
            _transformators.Add(transformator);
            upperSideNode.Connect(transformator);
            lowerSideNode.Connect(transformator);
        }

        public void AddLoad(string nodeName, string name, Complex power)
        {
            var node = GetNodeByNameInternal(nodeName);
            var load = new Load(name, power, node);
            _loads.Add(load);
            node.Connect(load);
        }

        public IList<ISet<INode>> GetSetsOfConnectedNodes()
        {
            var segments = new List<ISet<INode>>();

            if (_nodes.Count == 0)
                return segments;

            foreach (var node in _nodes)
            {
                var alreadyContained = segments.Count(segment => segment.Contains(node)) > 0;

                if (alreadyContained)
                    continue;

                var newSegment = new HashSet<INode>();
                node.AddConnectedNodes(newSegment);
                segments.Add(newSegment);
            }

            return segments;
        }

        public bool CheckIfFloatingNodesExists()
        {
            return GetSetsOfConnectedNodes().Count != 1;
        }

        public bool CheckIfNominalVoltagesDoNotMatch()
        {
            return _lines.Count(line => !line.NominalVoltagesMatch) > 0;
        }

        public INode GetNodeByName(string name)
        {
            return GetNodeByNameInternal(name);
        }

        #endregion

        #region private functions

        public Node GetNodeByNameInternal(string name)
        {
            Node result;
            _nodesByName.TryGetValue(name, out result);

            if (result == default(Node))
                throw new ArgumentOutOfRangeException("name", "specified node does not exist");

            return result;
        }

        #endregion
    }
}
