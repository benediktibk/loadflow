using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Numerics;
using System.Text;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Misc;

namespace SincalConnector
{
    public class PowerNetDatabaseAdapter
    {
        private readonly PowerNetComputable _powerNet;

        public PowerNetDatabaseAdapter(string database)
        {
            _powerNet = new PowerNetComputable();
            ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;

            using (var databaseConnection = new OleDbConnection(ConnectionString))
            {
                databaseConnection.Open();
                var commandFactory = new SqlCommandFactory(databaseConnection);

                DetermineFrequency(commandFactory);
                FetchNodes(commandFactory);
                FetchTerminals(commandFactory);

                var nodesByIds = _powerNet.NodeByNodeIds;
                var nodeIdsByElementIds = _powerNet.NodeIdsByElementIds;

                FetchFeedIns(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchLoads(commandFactory, nodeIdsByElementIds);
                FetchTransmissionLines(commandFactory, nodeIdsByElementIds, nodesByIds);
                FetchTwoWindingTransformers(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchThreeWindingTransformers(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchGenerators(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchSlackGenerators(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchImpedanceLoads(commandFactory, nodesByIds, nodeIdsByElementIds);
                FetchShuntReactors(commandFactory, nodesByIds, nodeIdsByElementIds);

                var notSupportedElementIds = FindNotSupportedElement(commandFactory);

                if (notSupportedElementIds.Count > 0)
                {
                    var message = new StringBuilder();
                    message.Append("the net contains ");
                    message.Append(notSupportedElementIds.Count);
                    message.Append(" not supported element(s): ");

                    for (var i = 0; i < notSupportedElementIds.Count - 1; ++i)
                        message.Append(notSupportedElementIds[i] + ", ");

                    message.Append(notSupportedElementIds.Last());
                    throw new NotSupportedException(message.ToString());
                }

                databaseConnection.Close();
            }

            if (_powerNet.CountOfElementsWithSlackBus != 1)
                throw new NotSupportedException("only one feedin and/or slack generator is supported");
        }

        public IReadOnlyPowerNetData Data
        {
            get { return _powerNet; }
        }

        public void AddLoad(int nodeId, Complex load)
        {
            _powerNet.Add(new Load(nodeId, load));
        }

        public string ConnectionString { get; private set; }

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator, out double relativePowerError)
        {
            Angle slackPhaseShift;
            IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds;
            var nodeResults = _powerNet.CalculateNodeVoltages(calculator, out slackPhaseShift, out nominalPhaseShiftByIds, out relativePowerError);

            if (nodeResults == null)
                return false;

            var nodeResultsCasted = nodeResults.ToDictionary(nodeResult => nodeResult.Key, nodeResult => new NodeResult(nodeResult.Key, nodeResult.Value.Voltage, nodeResult.Value.Power));
            _powerNet.FixNodeResults(nodeResultsCasted);

            using (var connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                var commandFactory = new SqlCommandFactory(connection);
                var deleteCommand = commandFactory.CreateCommandToDeleteAllNodeResults();
                var insertCommands = new List<OleDbCommand> { Capacity = Data.Nodes.Count };
                insertCommands.AddRange(Data.Nodes.Select(node => commandFactory.CreateCommandToAddResult(nodeResultsCasted[node.Id], node.NominalVoltage, nominalPhaseShiftByIds[node.Id], slackPhaseShift)));

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

            using (var databaseConnection = new OleDbConnection(ConnectionString))
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

            using (var databaseConnection = new OleDbConnection(ConnectionString))
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

        private void FetchTerminals(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllTerminals();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new Terminal(reader));
        }

        private void FetchNodes(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllNodes();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new Node(reader));
        }

        private void FetchTwoWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTwoWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new TwoWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchThreeWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllThreeWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new ThreeWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchTransmissionLines(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTransmissionLines();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new TransmissionLine(reader, nodeIdsByElementIds, nodesByIds, _powerNet.Frequency));
        }

        private void FetchLoads(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new Load(reader, nodeIdsByElementIds));
        }

        private void FetchImpedanceLoads(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllImpedanceLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new ImpedanceLoad(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchShuntReactors(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllShuntReactors();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new ShuntReactor(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchFeedIns(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllFeedIns();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchSlackGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllSlackGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _powerNet.Add(new SlackGenerator(reader, nodesByIds, nodeIdsByElementIds));
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

            _powerNet.Frequency = frequencies.First();
        }

        private List<int> FindNotSupportedElement(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllElementIdsSorted();
            var supportedElementList = _powerNet.AllSupportedElementIdsSorted;
            var supportedElementSet = new HashSet<int>(supportedElementList);
            var allElements = new List<int>();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    allElements.Add(reader.Parse<int>("Element_ID"));

            return allElements.Where(x => !supportedElementSet.Contains(x)).ToList();
        }
    }
}
