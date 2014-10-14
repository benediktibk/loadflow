using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using DatabaseHelper;

namespace SincalConnector
{
    public class PowerNet
    {
        #region variables

        private OleDbConnection _databaseConnection;
        private readonly double _frequency;
        private readonly IList<Terminal> _terminals; 
        private readonly IList<Node> _nodes;
        private readonly IList<FeedIn> _feedIns;
        private readonly IList<Load> _loads;
        private readonly IList<TransmissionLine> _transmissionLines; 

        #endregion

        #region constructor

        public PowerNet(string database)
        {
            _terminals = new List<Terminal>();
            _nodes = new List<Node>();
            _feedIns = new List<FeedIn>();
            _loads = new List<Load>();
            _transmissionLines = new List<TransmissionLine>();
            var nodesByIds = new Dictionary<int, IReadOnlyNode>();
            var nodeIdsByElementIds = new MultiDictionary<int, int>();

            var connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;
            _databaseConnection = new OleDbConnection(connectionString);
            _databaseConnection.Open();

            var frequenciesFetchCommand = CreateCommandToFetchAllFrequencies();
            frequenciesFetchCommand.Connection = _databaseConnection;
            var frequencies = new List<double>();
            
            using (var reader = new SafeDatabaseReader(frequenciesFetchCommand.ExecuteReader()))
                while (reader.Next())
                    frequencies.Add(reader.Parse<double>("f"));

            if (frequencies.Count != 1)
                throw new InvalidDataException("only one frequency per net is supported");

            _frequency = frequencies.First();

            var nodeFetchCommand = Node.CreateCommandToFetchAll();
            nodeFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(nodeFetchCommand.ExecuteReader()))
                while (reader.Next())
                {
                    var node = new Node(reader, _databaseConnection);
                    nodesByIds.Add(node.Id, node);
                    _nodes.Add(node);
                }

            var terminalFetchCommand = Terminal.CreateCommandToFetchAll();
            terminalFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(terminalFetchCommand.ExecuteReader()))
                while (reader.Next())
                {
                    var terminal = new Terminal(reader);
                    nodeIdsByElementIds.Add(terminal.ElementId, terminal.NodeId);
                    _terminals.Add(terminal);
                }

            var feedInFetchCommand = FeedIn.CreateCommandToFetchAll();
            feedInFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(feedInFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _feedIns.Add(new FeedIn(reader, nodesByIds, nodeIdsByElementIds));

            var loadFetchCommand = Load.CreateCommandToFetchAll();
            loadFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(loadFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _loads.Add(new Load(reader, nodeIdsByElementIds));

            var transmissionLineFetchCommand = TransmissionLine.CreateCommandToFetchAll();
            transmissionLineFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(transmissionLineFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _transmissionLines.Add(new TransmissionLine(reader, nodeIdsByElementIds, _frequency));
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

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAllFrequencies()
        {
            return new OleDbCommand("SELECT f FROM VoltageLevel GROUP BY f");
        }

        #endregion
    }
}
