using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
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
        private readonly IList<Transformer> _transformers;
        private readonly IList<Generator> _generators; 

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
                FetchTransformers(databaseConnection, nodesByIds, nodeIdsByElementIds);
                FetchGenerators(databaseConnection, nodesByIds, nodeIdsByElementIds);

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
            _transformers = new List<Transformer>();
            _generators = new List<Generator>();
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

        public IReadOnlyList<Transformer> Transformers
        {
            get { return new ReadOnlyCollection<Transformer>(_transformers); }
        }

        public IReadOnlyList<Generator> Generators
        {
            get { return new ReadOnlyCollection<Generator>(_generators); }
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
            var terminalFetchCommand = Terminal.CreateCommandToFetchAll();
            terminalFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(terminalFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _terminals.Add(new Terminal(reader));
        }

        private void FetchNodes(OleDbConnection databaseConnection)
        {
            var nodeFetchCommand = Node.CreateCommandToFetchAll();
            nodeFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(nodeFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _nodes.Add(new Node(reader, databaseConnection));
        }

        private void FetchTransformers(OleDbConnection databaseConnection, Dictionary<int, IReadOnlyNode> nodesByIds,
            MultiDictionary<int, int> nodeIdsByElementIds)
        {
            var transformerFetchCommand = Transformer.CreateCommandToFetchAll();
            transformerFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(transformerFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _transformers.Add(new Transformer(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchTransmissionLines(OleDbConnection databaseConnection, MultiDictionary<int, int> nodeIdsByElementIds)
        {
            var transmissionLineFetchCommand = TransmissionLine.CreateCommandToFetchAll();
            transmissionLineFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(transmissionLineFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _transmissionLines.Add(new TransmissionLine(reader, nodeIdsByElementIds, _frequency));
        }

        private void FetchLoads(OleDbConnection databaseConnection, MultiDictionary<int, int> nodeIdsByElementIds)
        {
            var loadFetchCommand = Load.CreateCommandToFetchAll();
            loadFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(loadFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _loads.Add(new Load(reader, nodeIdsByElementIds));
        }

        private void FetchFeedIns(OleDbConnection databaseConnection, Dictionary<int, IReadOnlyNode> nodesByIds, MultiDictionary<int, int> nodeIdsByElementIds)
        {
            var feedInFetchCommand = FeedIn.CreateCommandToFetchAll();
            feedInFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(feedInFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _feedIns.Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void FetchGenerators(OleDbConnection databaseConnection, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            var generatorFetchCommand = Generator.CreateCommandToFetchAll();
            generatorFetchCommand.Connection = databaseConnection;

            using (var reader = new SafeDatabaseReader(generatorFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _generators.Add(new Generator(reader, nodesByIds, nodeIdsByElementIds));
        }

        private void DetermineFrequency(OleDbConnection databaseConnection)
        {
            var frequenciesFetchCommand = CreateCommandToFetchAllFrequencies();
            frequenciesFetchCommand.Connection = databaseConnection;
            var frequencies = new List<double>();

            using (var reader = new SafeDatabaseReader(frequenciesFetchCommand.ExecuteReader()))
                while (reader.Next())
                    frequencies.Add(reader.Parse<double>("f"));

            if (frequencies.Count != 1)
                throw new NotSupportedException("only one frequency per net is supported");

            _frequency = frequencies.First();
        }

        private bool ContainsNotSupportedElement(OleDbConnection databaseConnection)
        {
            var elementFetchCommand = CreateCommandToFetchAllElementIdsSorted();
            elementFetchCommand.Connection = databaseConnection;
            var allSupportedElements = GetAllSupportedElementIdsSorted();
            var allElements = new List<int>();

            using (var reader = new SafeDatabaseReader(elementFetchCommand.ExecuteReader()))
                while (reader.Next())
                    allElements.Add(reader.Parse<int>("Element_ID"));

            if (allSupportedElements.Count != allElements.Count)
                return true;

            return allElements.Where((t, i) => t != allSupportedElements[i]).Any();
        }

        private List<int> GetAllSupportedElementIdsSorted()
        {
            var result = new List<int>(_feedIns.Count + _loads.Count + _transformers.Count + _transmissionLines.Count + _generators.Count);
            var originalCapacity = result.Capacity;

            result.AddRange(_feedIns.Select(element => element.Id));
            result.AddRange(_loads.Select(element => element.Id));
            result.AddRange(_transformers.Select(element => element.Id));
            result.AddRange(_transmissionLines.Select(element => element.Id));
            result.AddRange(_generators.Select(element => element.Id));

            if (originalCapacity != result.Capacity)
                throw new Exception("not enough space was allocated for the element IDs");

            result.Sort();
            return result;
        }

        #endregion
    }
}
