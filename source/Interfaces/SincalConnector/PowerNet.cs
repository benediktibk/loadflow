using System.Collections.Generic;
using System.Linq;
using Misc;

namespace SincalConnector
{
    public class PowerNet : IReadOnlyPowerNetData
    {
        private readonly IList<Terminal> _terminals;
        private readonly IList<Node> _nodes;
        private readonly IList<IReadOnlyNode> _readOnlyNodes;
        private readonly IList<INetElement> _netElements;
        private readonly IList<FeedIn> _feedIns;
        private readonly IList<Load> _loads;
        private readonly IList<TransmissionLine> _transmissionLines;
        private readonly IList<TwoWindingTransformer> _twoWindingTransformers;
        private readonly IList<Generator> _generators;
        private readonly IList<ImpedanceLoad> _impedanceLoads;
        private readonly IList<ThreeWindingTransformer> _threeWindingTransformers;
        private readonly IList<SlackGenerator> _slackGenerators;

        public PowerNet()
        {
            _terminals = new List<Terminal>();
            _nodes = new List<Node>();
            _readOnlyNodes = new List<IReadOnlyNode>();
            _netElements = new List<INetElement>();
            _feedIns = new List<FeedIn>();
            _loads = new List<Load>();
            _transmissionLines = new List<TransmissionLine>();
            _twoWindingTransformers = new List<TwoWindingTransformer>();
            _generators = new List<Generator>();
            _slackGenerators = new List<SlackGenerator>();
            _impedanceLoads = new List<ImpedanceLoad>();
            _threeWindingTransformers = new List<ThreeWindingTransformer>();
        }

        public double Frequency { get; set; }

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return (IReadOnlyList<IReadOnlyNode>)_readOnlyNodes; }
        }

        public IReadOnlyList<INetElement> NetElements
        {
            get { return (IReadOnlyList<INetElement>)_netElements; }
        }

        public IReadOnlyList<FeedIn> FeedIns
        {
            get { return (IReadOnlyList<FeedIn>) _feedIns; }
        }

        public IReadOnlyList<Load> Loads
        {
            get { return (IReadOnlyList<Load>) _loads; }
        }

        public IReadOnlyList<TransmissionLine> TransmissionLines
        {
            get { return (IReadOnlyList<TransmissionLine>) _transmissionLines; }
        }

        public IReadOnlyList<TwoWindingTransformer> TwoWindingTransformers
        {
            get { return (IReadOnlyList<TwoWindingTransformer>) _twoWindingTransformers; }
        }

        public IReadOnlyList<Generator> Generators
        {
            get { return (IReadOnlyList<Generator>) _generators; }
        }

        public IReadOnlyList<SlackGenerator> SlackGenerators
        {
            get { return (IReadOnlyList<SlackGenerator>) _slackGenerators; }
        }

        public IReadOnlyList<ImpedanceLoad> ImpedanceLoads
        {
            get { return (IReadOnlyList<ImpedanceLoad>) _impedanceLoads; }
        }

        public bool ContainsTransformers
        {
            get { return _twoWindingTransformers.Count + _threeWindingTransformers.Count > 0; }
        }

        public int CountOfElementsWithSlackBus
        {
            get { return _feedIns.Count + _slackGenerators.Count; }
        }

        public void Add(Terminal terminal)
        {
            _terminals.Add(terminal);
        }

        public void Add(Node node)
        {
            _nodes.Add(node);
            _readOnlyNodes.Add(node);
        }

        public void Add(TwoWindingTransformer element)
        {
            _netElements.Add(element);
            _twoWindingTransformers.Add(element);
        }

        public void Add(ThreeWindingTransformer element)
        {
            _netElements.Add(element);
            _threeWindingTransformers.Add(element);
        }

        public void Add(TransmissionLine element)
        {
            _netElements.Add(element);
            _transmissionLines.Add(element);
        }

        public void Add(Load element)
        {
            _netElements.Add(element);
            _loads.Add(element);
        }

        public void Add(ImpedanceLoad element)
        {
            _netElements.Add(element);
            _impedanceLoads.Add(element);
        }

        public void Add(FeedIn element)
        {
            _netElements.Add(element);
            _feedIns.Add(element);
        }

        public void Add(Generator element)
        {
            _netElements.Add(element);
            _generators.Add(element);
        }

        public void Add(SlackGenerator element)
        {
            _netElements.Add(element);
            _slackGenerators.Add(element);
        }

        public List<int> GetAllSupportedElementIdsSorted()
        {
            return _netElements.Select(element => element.Id).OrderBy(id => id).ToList();
        }

        public MultiDictionary<int, ImpedanceLoad> GetImpedanceLoadsByNodeId()
        {
            var result = new MultiDictionary<int, ImpedanceLoad>();

            foreach (var impedanceLoad in _impedanceLoads)
                result.Add(impedanceLoad.NodeId, impedanceLoad);

            return result;
        }

        public MultiDictionary<int, int> CreateDictionaryNodeIdsByElementIds()
        {
            var nodeIdsByElementIds = new MultiDictionary<int, int>();
            foreach (var terminal in _terminals)
                nodeIdsByElementIds.Add(terminal.ElementId, terminal.NodeId);
            return nodeIdsByElementIds;
        }

        public Dictionary<int, IReadOnlyNode> CreateDictionaryNodeByIds()
        {
            return _nodes.ToDictionary<Node, int, IReadOnlyNode>(node => node.Id, node => node);
        }
    }
}
