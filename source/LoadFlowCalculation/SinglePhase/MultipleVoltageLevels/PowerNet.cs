using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class PowerNet : IReadOnlyPowerNet
    {
        #region variables

        private readonly double _frequency;
        private readonly List<Load> _loads;
        private readonly List<Line> _lines;
        private readonly List<Transformer> _transformers;
        private readonly List<Generator> _generators;
        private readonly List<FeedIn> _feedIns;
        private readonly List<IPowerNetElement> _elements; 
        private readonly List<Node> _nodes;
        private readonly Dictionary<string, Node> _nodesByName;
        private readonly HashSet<string> _allNames;
        private readonly Node _groundNode;
        private readonly FeedIn _groundFeedIn;

        #endregion

        #region public functions

        public PowerNet(double frequency)
        {
            _frequency = frequency;
            _loads = new List<Load>();
            _lines = new List<Line>();
            _transformers = new List<Transformer>();
            _generators = new List<Generator>();
            _feedIns = new List<FeedIn>();
            _elements = new List<IPowerNetElement>();
            _nodes = new List<Node>();
            _nodesByName = new Dictionary<string, Node>();
            _allNames = new HashSet<string>();
            _groundNode = new Node("#GROUNDNODE", 0);
            _groundFeedIn = new FeedIn("#GROUNDFEEDIN", _groundNode, new Complex(0, 0), 0);
            _groundNode.Connect(_groundFeedIn);
            _allNames.Add(_groundNode.Name);
            _allNames.Add(_groundFeedIn.Name);
            _nodesByName.Add(_groundNode.Name, _groundNode);
        }

        public IList<ISet<IExternalReadOnlyNode>> GetSetsOfConnectedNodes()
        {
            var segments = new List<ISet<IExternalReadOnlyNode>>();

            if (_nodes.Count == 0)
                return segments;

            foreach (var node in _nodes)
            {
                var alreadyContained = segments.Count(segment => segment.Contains(node)) > 0;

                if (alreadyContained)
                    continue;

                var newSegment = new HashSet<IExternalReadOnlyNode>();
                node.AddConnectedNodes(newSegment);
                segments.Add(newSegment);
            }

            return segments;
        }

        public bool CalculateNodeVoltages(LoadFlowCalculator calculator)
        {
            var voltages = calculator.CalculateNodeVoltages(this);

            if (voltages == null)
                return true;

            foreach (var node in _nodes)
                node.UpdateVoltage(voltages);

            return false;
        }

        #endregion

        #region add functions

        public void AddNode(string name, double nominalVoltage)
        {
            if (_nodesByName.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "a node with this name already exists");
            AddName(name);

            var node = new Node(name, nominalVoltage);
            _nodes.Add(node);
            _nodesByName.Add(name, node);
        }

        public void AddLine(string name, string sourceNodeName, string targetNodeName, double lengthResistance, double lengthInductance,
            double shuntConductance, double capacity)
        {
            AddName(name);
            var sourceNode = GetNodeByNameInternal(sourceNodeName);
            var targetNode = GetNodeByNameInternal(targetNodeName);
            var line = new Line(name, sourceNode, targetNode, lengthResistance, lengthInductance, shuntConductance, capacity, _frequency);
            _lines.Add(line);
            _elements.Add(line);
            sourceNode.Connect(line);
            targetNode.Connect(line);
        }

        public void AddGenerator(string nodeName, string name, double voltageMagnitude, double realPower)
        {
            AddName(name);
            var node = GetNodeByNameInternal(nodeName);
            var generator = new Generator(name, node, voltageMagnitude, realPower);
            _generators.Add(generator);
            _elements.Add(generator);
            node.Connect(generator);
        }

        public void AddFeedIn(string nodeName, string name, Complex voltage, double shortCircuitPower)
        {
            AddName(name);
            var node = GetNodeByNameInternal(nodeName);
            var feedIn = new FeedIn(name, node, voltage, shortCircuitPower);
            _feedIns.Add(feedIn);
            _elements.Add(feedIn);
            node.Connect(feedIn);
        }

        public void AddTransformer(string upperSideNodeName, string lowerSideNodeName, string name, Complex upperSideImpedance, Complex lowerSideImpedance, Complex mainImpedance, double ratio, double idealTransformerFactor)
        {
            AddName(name);
            var upperSideNode = GetNodeByNameInternal(upperSideNodeName);
            var lowerSideNode = GetNodeByNameInternal(lowerSideNodeName);
            var transformer = new Transformer(name, upperSideNode, lowerSideNode, upperSideImpedance, lowerSideImpedance, mainImpedance, ratio, idealTransformerFactor);
            _transformers.Add(transformer);
            _elements.Add(transformer);
            upperSideNode.Connect(transformer);
            lowerSideNode.Connect(transformer);
        }

        public void AddLoad(string nodeName, string name, Complex power)
        {
            AddName(name);
            var node = GetNodeByNameInternal(nodeName);
            var load = new Load(name, power, node);
            _loads.Add(load);
            _elements.Add(load);
            node.Connect(load);
        }

        #endregion

        #region IReadOnlyPowerNet functions

        public bool CheckIfFloatingNodesExists()
        {
            return GetSetsOfConnectedNodes().Count != 1;
        }

        public bool CheckIfNominalVoltagesDoNotMatch()
        {
            return _elements.Exists(element => !element.NominalVoltagesMatch);
        }

        public bool CheckIfNodeIsOverdetermined()
        {
            return _nodes.Count(x => x.IsOverdetermined) > 0;
        }

        public bool CheckIfGroundNodeIsNecessary()
        {
            return _elements.Exists(x => x.NeedsGroundNode);
        }

        public IExternalReadOnlyNode GetNodeByName(string name)
        {
            return GetNodeByNameInternal(name);
        }

        public IReadOnlyList<IReadOnlyNode> GetAllNecessaryNodes()
        {
            var allNodes = new List<IReadOnlyNode>();

            foreach (var element in _elements)
                allNodes.AddRange(element.GetInternalNodes());
            allNodes.AddRange(_nodes);

            if (CheckIfGroundNodeIsNecessary())
            {
                allNodes.AddRange(_groundFeedIn.GetInternalNodes());
                allNodes.Add(_groundNode);
            }

            return allNodes;
        }

        public void FillInAdmittances(AdmittanceMatrix admittances, double scaleBasePower)
        {
            foreach (var element in _elements)
                element.FillInAdmittances(admittances, scaleBasePower, _groundNode);
        }

        #endregion
        
        #region IReadOnlyPowerNet properties

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

        public int TransformerCount
        {
            get { return _transformers.Count; }
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

        #endregion

        #region private functions

        private Node GetNodeByNameInternal(string name)
        {
            Node result;
            _nodesByName.TryGetValue(name, out result);

            if (result == default(Node))
                throw new ArgumentOutOfRangeException("name", "specified node does not exist");

            return result;
        }

        private void AddName(string name)
        {
            if (_allNames.Contains(name))
                throw new ArgumentOutOfRangeException("name", "the name must be unique throughout all net elements");
            if (name.Contains('#'))
                throw new ArgumentOutOfRangeException("name", "the name must not contain a #");
            _allNames.Add(name);
        }

        #endregion
    }
}
