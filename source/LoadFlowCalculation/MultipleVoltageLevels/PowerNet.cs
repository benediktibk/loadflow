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
            _nodes = new List<Node>();
            _nodesByName = new Dictionary<string, Node>();
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

        public void AddLine(string name, string firstNode, string secondNode, double lengthResistance, double lengthInductance,
            double shuntConductance, double capacity)
        {
            var sourceNode = GetNodeByNameInternal(firstNode);
            var targetNode = GetNodeByNameInternal(secondNode);
            var line = new Line(name, sourceNode, targetNode);
            _lines.Add(line);
            sourceNode.Connect(line);
            targetNode.Connect(line);
        }

        public void AddGenerator(string node, string name, double synchronLengthInductance, double synchronousGeneratedVoltage)
        {
            throw new NotImplementedException();
        }

        public void AddFeedIn(string node, string name, double shortCircuitPower)
        {
            throw new NotImplementedException();
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

        public void AddLoad(string node, string name, Complex power)
        {
            throw new NotImplementedException();
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
