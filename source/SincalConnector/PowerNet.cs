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

        private OleDbConnection _databaseConnection;
        private readonly IList<Node> _nodes;
        private readonly IList<FeedIn> _feedIns;
        private readonly IList<Terminal> _terminals; 

        #endregion

        #region constructor

        public PowerNet(string database)
        {
            _nodes = new List<Node>();
            _feedIns = new List<FeedIn>();
            _terminals = new List<Terminal>();
            var nodesByIds = new Dictionary<int, IReadOnlyNode>();
            var nodeIdsByElementIds = new Dictionary<int, int>();

            var connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;
            _databaseConnection = new OleDbConnection(connectionString);
            _databaseConnection.Open();

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
        }

        #endregion

        #region public functions

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IReadOnlyNode>().ToList(); }
        }

        public IReadOnlyList<FeedIn> FeedIns
        {
            get { return new ReadOnlyCollection<FeedIn>(_feedIns); }
        }

        #endregion
    }
}
