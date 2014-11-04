using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace SincalConnector
{
    public class PowerNet
    {
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

        public PowerNet(string database) : this()
        {
            _connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;

            using (var databaseConnection = new OleDbConnection(_connectionString))
            {
                databaseConnection.Open();
                var commandFactory = new SqlCommandFactory(databaseConnection);

                DetermineFrequency(commandFactory);
                FetchNodes(commandFactory);
                FetchTerminals(commandFactory);

                var nodesByIds = CreateDictionaryNodeByIds();
                var nodeIdsByElementIds = CreateDictionaryNodeIdsByElementIds();

                FetchFeedIns(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchLoads(commandFactory, nodeIdsByElementIds);
                FetchTransmissionLines(commandFactory, nodeIdsByElementIds);
                FetchTwoWindingTransformers(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchThreeWindingTransformers(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchGenerators(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchSlackGenerators(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchImpedanceLoads(commandFactory, nodesByIds, nodeIdsByElementIds);

                if (ContainsNotSupportedElement(commandFactory))
                    throw new NotSupportedException("the net contains a not supported element");

                databaseConnection.Close();
            }

            if (_feedIns.Count + _slackGenerators.Count != 1)
                throw new NotSupportedException("only one feedin and/or slack generator is supported");
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

        public bool ContainsTransformers
        {
            get { return _twoWindingTransformers.Count + _threeWindingTransformers.Count > 0; }
        }

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator)
        {
            var symmetricPowerNet = CreateSymmetricPowerNet(calculator);
            var impedanceLoadsByNodeId = GetImpedanceLoadsByNodeId();
            var nominalPhaseShifts = symmetricPowerNet.CalculateNominalPhaseShiftPerNode();
            var slackPhaseShift = ContainsTransformers ? symmetricPowerNet.SlackPhaseShift : new Angle();
            var nominalPhaseShiftByIds = nominalPhaseShifts.ToDictionary(nominalPhaseShift => nominalPhaseShift.Key.Id, nominalPhaseShift => nominalPhaseShift.Value);
            var nodeResults = symmetricPowerNet.CalculateNodeVoltages();

            if (nodeResults == null)
                return false;

            foreach (var node in _nodes)
                node.SetResult(nodeResults[node.Id].Voltage, nodeResults[node.Id].Power,
                    impedanceLoadsByNodeId.Get(node.Id));

            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                var commandFactory = new SqlCommandFactory(connection);
                var deleteCommand = commandFactory.CreateCommandToDeleteAllNodeResults();
                var insertCommands = new List<OleDbCommand> { Capacity = _nodes.Count };
                insertCommands.AddRange(_nodes.Select(node => commandFactory.CreateCommandToAddResult(node, nominalPhaseShiftByIds[node.Id], slackPhaseShift)));

                deleteCommand.ExecuteNonQuery();

                foreach (var command in insertCommands)
                    command.ExecuteNonQuery();

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
                var commandFactory = new SqlCommandFactory(databaseConnection);
                var command = commandFactory.CreateCommandToFetchAllNodeResults();

                using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                    while (reader.Next())
                        results.Add(new NodeResult(reader));

                databaseConnection.Close();
            }

            return results;
        }

        public IList<NodeResultTableEntry> GetNodeResultTableEntriesFromDatabase()
        {
            var results = new List<NodeResultTableEntry>();

            using (var databaseConnection = new OleDbConnection(_connectionString))
            {
                databaseConnection.Open();
                var commandFactory = new SqlCommandFactory(databaseConnection);
                var command = commandFactory.CreateCommandToFetchAllNodeResultTableEntries();

                using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                    while (reader.Next())
                        results.Add(new NodeResultTableEntry(reader));

                databaseConnection.Close();
            }

            return results;
        }

        private MultiDictionary<int, ImpedanceLoad> GetImpedanceLoadsByNodeId()
        {
            var result = new MultiDictionary<int, ImpedanceLoad>();

            foreach (var impedanceLoad in _impedanceLoads)
                result.Add(impedanceLoad.NodeId, impedanceLoad);

            return result;
        }

        private MultiDictionary<int, int> CreateDictionaryNodeIdsByElementIds()
        {
            var nodeIdsByElementIds = new MultiDictionary<int, int>();
            foreach (var terminal in _terminals)
                nodeIdsByElementIds.Add(terminal.ElementId, terminal.NodeId);
            return nodeIdsByElementIds;
        }

        private Dictionary<int, IReadOnlyNode> CreateDictionaryNodeByIds()
        {
            return _nodes.ToDictionary<Node, int, IReadOnlyNode>(node => node.Id, node => node);
        }

        private void FetchTerminals(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllTerminals();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _terminals.Add(new Terminal(reader));
        }

        private void FetchNodes(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllNodes();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _nodes.Add(new Node(reader, null));
        }

        private void FetchTwoWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTwoWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new TwoWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(TwoWindingTransformer element)
        {
            _netElements.Add(element);
            _twoWindingTransformers.Add(element);
        }

        private void FetchThreeWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllThreeWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new ThreeWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(ThreeWindingTransformer element)
        {
            _netElements.Add(element);
            _threeWindingTransformers.Add(element);
        }

        private void FetchTransmissionLines(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTransmissionLines();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new TransmissionLine(reader, nodeIdsByElementIds, Frequency));
        }

        private void Add(TransmissionLine element)
        {
            _netElements.Add(element);
            _transmissionLines.Add(element);
        }

        private void FetchLoads(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new Load(reader, nodeIdsByElementIds));
        }

        private void Add(Load element)
        {
            _netElements.Add(element);
            _loads.Add(element);
        }

        private void FetchImpedanceLoads(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllImpedanceLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new ImpedanceLoad(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(ImpedanceLoad element)
        {
            _netElements.Add(element);
            _impedanceLoads.Add(element);
        }

        private void FetchFeedIns(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllFeedIns();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(FeedIn element)
        {
            _netElements.Add(element);
            _feedIns.Add(element);
        }

        private void FetchGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(Generator element)
        {
            _netElements.Add(element);
            _generators.Add(element);
        }

        private void FetchSlackGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllSlackGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    Add(new SlackGenerator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void Add(SlackGenerator element)
        {
            _netElements.Add(element);
            _slackGenerators.Add(element);
        }

        private void DetermineFrequency(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllFrequencies();
            var frequencies = new List<double>();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    frequencies.Add(reader.Parse<double>("f"));

            if (frequencies.Count != 1)
                throw new NotSupportedException("only one frequency per net is supported");

            Frequency = frequencies.First();
        }

        private bool ContainsNotSupportedElement(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllElementIdsSorted();
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

        private SymmetricPowerNet CreateSymmetricPowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            var singlePhasePowerNet = new PowerNetComputable(Frequency, new PowerNetFactory(nodeVoltageCalculator), new NodeGraph());
            var symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);

            foreach (var node in _nodes)
                node.AddTo(symmetricPowerNet);

            foreach (var element in _netElements)
                element.AddTo(symmetricPowerNet);

            return symmetricPowerNet;
        }
    }
}
