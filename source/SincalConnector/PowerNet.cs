using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using DatabaseHelper;

namespace SincalConnector
{
    public class PowerNet
    {
        #region variables

        private readonly IList<Terminal> _terminals;
        private readonly IList<Node> _nodes;
        private readonly IList<INetElement> _netElements;
        private readonly IList<FeedIn> _feedIns;
        private readonly IList<Load> _loads;
        private readonly IList<TransmissionLine> _transmissionLines;
        private readonly IList<TwoWindingTransformer> _twoWindingTransformers;
        private readonly IList<Generator> _generators;
        private readonly IList<ImpedanceLoad> _impedanceLoads;
        private readonly IList<ThreeWindingTransformer> _threeWindingTransformers;
        private readonly IList<SlackGenerator> _slackGenerators;
        private readonly string _connectionString;

        #endregion

        #region constructor

        public PowerNet(string database) : this()
        {
            _connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;

            using (var databaseConnection = new OleDbConnection(_connectionString))
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

                databaseConnection.Close();
            }
        }

        private PowerNet()
        {
            _terminals = new List<Terminal>();
            _nodes = new List<Node>();
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

        #endregion

        #region properties

        public double Frequency { get; private set; }

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IReadOnlyNode>().ToList(); }
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

        public IReadOnlyList<ImpedanceLoad> ImpedanceLoads
        {
            get { return new ReadOnlyCollection<ImpedanceLoad>(_impedanceLoads); }
        }

        #endregion

        #region public functions

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet();
            var success = symmetricPowerNet.CalculateNodeVoltages(calculator);

            if (!success)
                return false;

            foreach (var node in _nodes)
                node.SetResult(symmetricPowerNet.GetNodeVoltage(node.Id), symmetricPowerNet.GetNodePower(node.Id)*(-1));

            var insertCommands = new List<OleDbCommand>() { Capacity = _nodes.Count };
            insertCommands.AddRange(_nodes.Select(node => node.CreateCommandToAddResult(0)));

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();

                var deleteCommand = NodeResult.CreateCommandToDeleteAll();
                deleteCommand.Connection = connection;
                deleteCommand.ExecuteNonQuery();

                foreach (var command in insertCommands)
                {
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            return true;
        }

        public IList<NodeResult> GetNodeResultsFromDatabase()
        {
            var results = new List<NodeResult>();

            using (var databaseConnection = new OleDbConnection(_connectionString))
            {
                databaseConnection.Open();

                var command = NodeResult.CreateCommandToFetchAll();
                command.Connection = databaseConnection;

                using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                    while (reader.Next())
                        results.Add(new NodeResult(reader));

                databaseConnection.Close();
            }

            return results;
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
                    Add(new TwoWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(TwoWindingTransformer element)
        {
            _netElements.Add(element);
            _twoWindingTransformers.Add(element);
        }

        private void FetchThreeWindingTransformers(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = ThreeWindingTransformer.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new ThreeWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(ThreeWindingTransformer element)
        {
            _netElements.Add(element);
            _threeWindingTransformers.Add(element);
        }

        private void FetchTransmissionLines(OleDbConnection databaseConnection, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = TransmissionLine.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new TransmissionLine(reader, nodeIdsByElementIds, Frequency));
        }

        private void Add(TransmissionLine element)
        {
            _netElements.Add(element);
            _transmissionLines.Add(element);
        }

        private void FetchLoads(OleDbConnection databaseConnection, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = Load.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new Load(reader, nodeIdsByElementIds));
        }

        private void Add(Load element)
        {
            _netElements.Add(element);
            _loads.Add(element);
        }

        private void FetchImpedanceLoads(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = ImpedanceLoad.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new ImpedanceLoad(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(ImpedanceLoad element)
        {
            _netElements.Add(element);
            _impedanceLoads.Add(element);
        }

        private void FetchFeedIns(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = FeedIn.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(FeedIn element)
        {
            _netElements.Add(element);
            _feedIns.Add(element);
        }

        private void FetchGenerators(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = Generator.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(Generator element)
        {
            _netElements.Add(element);
            _generators.Add(element);
        }

        private void FetchSlackGenerators(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = SlackGenerator.CreateCommandToFetchAll();
            command.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new SlackGenerator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(SlackGenerator element)
        {
            _netElements.Add(element);
            _slackGenerators.Add(element);
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

            Frequency = frequencies.First();
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
            return _netElements.Select(element => element.Id).OrderBy(id => id).ToList();
        }

        private SymmetricPowerNet CreateSymmetricPowerNet()
        {
            var result = new SymmetricPowerNet(Frequency);

            foreach (var node in _nodes)
                node.AddTo(result);

            foreach (var element in _netElements)
                element.AddTo(result);

            return result;
        }

        #endregion
    }
}
