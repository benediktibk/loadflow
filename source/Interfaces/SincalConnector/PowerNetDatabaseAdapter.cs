using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using Misc;

namespace SincalConnector
{
    public class PowerNetDatabaseAdapter
    {
        private readonly PowerNet _data;

        public PowerNetDatabaseAdapter(string database)
        {
            _data = new PowerNet();
            ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;

            using (var databaseConnection = new OleDbConnection(ConnectionString))
            {
                databaseConnection.Open();
                var commandFactory = new SqlCommandFactory(databaseConnection);

                DetermineFrequency(commandFactory);
                FetchNodes(commandFactory);
                FetchTerminals(commandFactory);

                var nodesByIds = _data.CreateDictionaryNodeByIds();
                var nodeIdsByElementIds = _data.CreateDictionaryNodeIdsByElementIds();

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

            if (_data.CountOfElementsWithSlackBus != 1)
                throw new NotSupportedException("only one feedin and/or slack generator is supported");
        }

        public IReadOnlyPowerNetData Data
        {
            get { return _data; }
        }

        public string ConnectionString { get; private set; }

        public void SetNodeResults(IReadOnlyDictionary<long, Calculation.NodeResult> nodeResults)
        {
            _data.SetNodeResults(nodeResults);
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
                    _data.Add(new Terminal(reader));
        }

        private void FetchNodes(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllNodes();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new Node(reader, null));
        }

        private void FetchTwoWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTwoWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new TwoWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchThreeWindingTransformers(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds,
            IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllThreeWindingTransformers();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new ThreeWindingTransformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchTransmissionLines(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllTransmissionLines();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new TransmissionLine(reader, nodeIdsByElementIds, _data.Frequency));
        }

        private void FetchLoads(SqlCommandFactory commandFactory, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new Load(reader, nodeIdsByElementIds));
        }

        private void FetchImpedanceLoads(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllImpedanceLoads();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new ImpedanceLoad(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchFeedIns(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllFeedIns();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchSlackGenerators(SqlCommandFactory commandFactory, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var command = commandFactory.CreateCommandToFetchAllSlackGenerators();

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    _data.Add(new SlackGenerator(reader, nodesByIds, nodeIdsByElementIds));
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

            _data.Frequency = frequencies.First();
        }

        private bool ContainsNotSupportedElement(SqlCommandFactory commandFactory)
        {
            var command = commandFactory.CreateCommandToFetchAllElementIdsSorted();
            var allSupportedElements = _data.GetAllSupportedElementIdsSorted();
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
