using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics;
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

                if (ContainsNotSupportedElement(commandFactory))
                    throw new NotSupportedException("the net contains a not supported element");

                databaseConnection.Close();
            }

            if (_powerNet.CountOfElementsWithSlackBus != 1)
                throw new NotSupportedException("only one feedin and/or slack generator is supported");
        }

        public IReadOnlyPowerNetData Data
        {
            get { return _powerNet; }
        }

        public string ConnectionString { get; private set; }

        public bool CalculateNodeVoltages(INodeVoltageCalculator calculator, double powerFactor, out double relativePowerError)
        {
            Angle slackPhaseShift;
            IReadOnlyDictionary<int, Angle> nominalPhaseShiftByIds;
            var nodeResults = _powerNet.CalculateNodeVoltages(calculator, powerFactor, out slackPhaseShift, out nominalPhaseShiftByIds, out relativePowerError);

            if (nodeResults == null)
                return false;

            var impedanceLoadsByNodeId = _powerNet.ImpedanceLoadsByNodeId;
            var nodeResultsModified = new Dictionary<int, NodeResult>();

            foreach (var node in _powerNet.Nodes)
            {
                var impedanceLoads = impedanceLoadsByNodeId.Get(node.Id);
                var loadByImpedances = new Complex();
                var nodeResult = nodeResults[node.Id];

                foreach (var impedanceLoad in impedanceLoads)
                {
                    var impedance = impedanceLoad.Impedance;
                    var loadByImpedance = nodeResult.Voltage * (nodeResult.Voltage / impedance).Conjugate();
                    loadByImpedances = loadByImpedances + loadByImpedance;
                }

                var nodeResultModified = new NodeResult(node.Id, nodeResult.Voltage, nodeResult.Power - loadByImpedances);
                nodeResultsModified.Add(node.Id, nodeResultModified);
            }

            using (var connection = new OleDbConnection(ConnectionString))
            {
                connection.Open();
                var commandFactory = new SqlCommandFactory(connection);
                var deleteCommand = commandFactory.CreateCommandToDeleteAllNodeResults();
                var insertCommands = new List<OleDbCommand> { Capacity = Data.Nodes.Count };
                insertCommands.AddRange(Data.Nodes.Select(node => commandFactory.CreateCommandToAddResult(nodeResultsModified[node.Id], node.NominalVoltage, nominalPhaseShiftByIds[node.Id], slackPhaseShift)));

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

        private bool ContainsNotSupportedElement(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllElementIdsSorted();
            var allSupportedElements = _powerNet.AllSupportedElementIdsSorted;
            var allElements = new List<int>();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    allElements.Add(reader.Parse<int>("Element_ID"));

            if (allSupportedElements.Count != allElements.Count)
                return true;

            return allElements.Where((t, i) => t != allSupportedElements[i]).Any();
        }
    }
}
