using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using DatabaseHelper;

namespace SincalConnector
{
    public class PowerNet
    {
        #region variables

        private double _frequency;
        private readonly IList<Terminal> _terminals;
        private readonly IList<Node> _nodes;
        private readonly IList<FeedIn> _feedIns;
        private readonly IList<Load> _loads;
        private readonly IList<TransmissionLine> _transmissionLines;
        private readonly IList<TwoWindingTransformer> _twoWindingTransformers;
        private readonly IList<Generator> _generators;
        private readonly IList<ImpedanceLoad> _impedanceLoads;
        private readonly IList<ThreeWindingTransformer> _threeWindingTransformers;
        private readonly IList<SlackGenerator> _slackGenerators; 

        #endregion

        #region constructor

        public PowerNet(string database) : this()
        {
            var connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;

            using (var databaseConnection = new OleDbConnection(connectionString))
            {
                databaseConnection.Open();

                DetermineFrequency(databaseConnection);
                FetchNodes(databaseConnection);
                FetchTerminals(databaseConnection);

                var nodesByIds = CreateDictionaryNodeByIds();
                var nodeIdsByElementIds = CreateDictionaryNodeIdsByElementIds();

                FetchFeedIns(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchLoads(databaseConnection, nodeIdsByElementIds);
                FetchTransmissionLines(databaseConnection, nodeIdsByElementIds);
                FetchTwoWindingTransformers(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchThreeWindingTransformers(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchGenerators(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchSlackGenerators(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchImpedanceLoads(databaseConnection, nodesByIds, nodeIdsByElementIds);

                if (ContainsNotSupportedElement(databaseConnection))
                    throw new NotSupportedException("the net contains a not supported element");
            }
        }

        private PowerNet()
        {
            _terminals = new List<Terminal>();
            _nodes = new List<Node>();
            _feedIns = new List<FeedIn>();
            _loads = new List<Load>();
            _transmissionLines = new List<TransmissionLine>();
            _twoWindingTransformers = new List<TwoWindingTransformer>();
            _generators = new List<Generator>();
            _slackGenerators = new List<SlackGenerator>();
            _impedanceLoads = new List<ImpedanceLoad>();
            _threeWindingTransformers = new List<ThreeWindingTransformer>();
        }

        #endregion

        #region properties

        public double Frequency
        {
            get { return _frequency; }
        }

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IReadOnlyNode>().ToList(); }
        }

        public IList<INetElement> NetElements
        {
            get
            {
                var result = new List<INetElement>(
                    _feedIns.Count + _loads.Count + _twoWindingTransformers.Count + _transmissionLines.Count + _generators.Count +
                    _impedanceLoads.Count + _threeWindingTransformers.Count + _slackGenerators.Count);
                var originalCapacity = result.Capacity;

                result.AddRange(_feedIns);
                result.AddRange(_loads);
                result.AddRange(_twoWindingTransformers);
                result.AddRange(_transmissionLines);
                result.AddRange(_generators);
                result.AddRange(_impedanceLoads);
                result.AddRange(_threeWindingTransformers);
                result.AddRange(_slackGenerators);

                if (originalCapacity != result.Capacity)
                    throw new Exception("not enough space was allocated for the net elements");

                return result;
            }
        }

        public IReadOnlyList<FeedIn> FeedIns
        {
            get { return new ReadOnlyCollection<FeedIn>(_feedIns); }
        }

        public IReadOnlyList<Load> Loads
        {
            get { return new ReadOnlyCollection<Load>(_loads); }
        }

        public IReadOnlyList<TransmissionLine> TransmissionLines
        {
            get { return new ReadOnlyCollection<TransmissionLine>(_transmissionLines); }
        }

        public IReadOnlyList<TwoWindingTransformer> TwoWindingTransformers
        {
            get { return new ReadOnlyCollection<TwoWindingTransformer>(_twoWindingTransformers); }
        }

        public IReadOnlyList<Generator> Generators
        {
            get { return new ReadOnlyCollection<Generator>(_generators); }
        }

        public IReadOnlyList<SlackGenerator> SlackGenerators
        {
            get { return new ReadOnlyCollection<SlackGenerator>(_slackGenerators); }
        }

        #endregion

        #region public functions

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator)
        {
            return true;
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAllFrequencies()
        {
            return new OleDbCommand("SELECT f FROM VoltageLevel GROUP BY f;");
        }

        public static OleDbCommand CreateCommandToFetchAllElementIdsSorted()
        {
            return new OleDbCommand("SELECT Element_ID FROM Element ORDER BY Element_ID ASC;");
        }

        #endregion

        #region private functions

        private MultiDictionary<int, int> CreateDictionaryNodeIdsByElementIds()
        {
            var nodeIdsByElementIds = new MultiDictionary<int, int>();
            foreach (var terminal in _terminals)
                nodeIdsByElementIds.Add(terminal.ElementId, terminal.NodeId);
            return nodeIdsByElementIds;
        }

        private Dictionary<int, IReadOnlyNode> CreateDictionaryNodeByIds()
        {
            var nodesByIds = new Dictionary<int, IReadOnlyNode>();
            foreach (var node in _nodes)
                nodesByIds.Add(node.Id, node);
            return nodesByIds;
        }

        private void FetchTerminals(OleDbConnection databaseConnection)
        {
            var command = Terminal.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _terminals.Add(new Terminal(reader));
        }

        private void FetchNodes(OleDbConnection databaseConnection)
        {
            var command = Node.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _nodes.Add(new Node(reader, databaseConnection));
        }

        private void FetchTwoWindingTransformers(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = TwoWindingTransformer.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _twoWindingTransformers.Add(new TwoWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchThreeWindingTransformers(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = ThreeWindingTransformer.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _threeWindingTransformers.Add(new ThreeWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchTransmissionLines(OleDbConnection databaseConnection, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = TransmissionLine.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _transmissionLines.Add(new TransmissionLine(reader, nodeIdsByElementIds, _frequency));
        }

        private void FetchLoads(OleDbConnection databaseConnection, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = Load.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _loads.Add(new Load(reader, nodeIdsByElementIds));
        }

        private void FetchImpedanceLoads(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = ImpedanceLoad.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _impedanceLoads.Add(new ImpedanceLoad(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchFeedIns(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = FeedIn.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _feedIns.Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchGenerators(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = Generator.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _generators.Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchSlackGenerators(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = SlackGenerator.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _slackGenerators.Add(new SlackGenerator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void DetermineFrequency(OleDbConnection databaseConnection)
        {
            var command = CreateCommandToFetchAllFrequencies();
            command.Connection = databaseConnection;
            var frequencies = new List<double>();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    frequencies.Add(reader.Parse<double>("f"));

            if (frequencies.Count != 1)
                throw new NotSupportedException("only one frequency per net is supported");

            _frequency = frequencies.First();
        }

        private bool ContainsNotSupportedElement(OleDbConnection databaseConnection)
        {
            var command = CreateCommandToFetchAllElementIdsSorted();
            command.Connection = databaseConnection;
            var allSupportedElements = GetAllSupportedElementIdsSorted();
            var allElements = new List<int>();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    allElements.Add(reader.Parse<int>("Element_ID"));

            if (allSupportedElements.Count != allElements.Count)
                return true;

            return allElements.Where((t, i) => t != allSupportedElements[i]).Any();
        }

        private List<int> GetAllSupportedElementIdsSorted()
        {
            return NetElements.Select(element => element.Id).OrderBy(id => id).ToList();
        }

        private SymmetricPowerNet CreateSymmetricPowerNet()
        {
            var result = new SymmetricPowerNet(_frequency);
            return result;
        }

        #endregion
    }
}
