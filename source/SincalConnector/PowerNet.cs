using System.Collections;
using System.Collections.Generic;
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

        #endregion

        #region constructor

        public PowerNet(string database)
        {
            _nodes = new List<Node>();

            var connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + database;
            _databaseConnection = new OleDbConnection(connectionString);
            _databaseConnection.Open();

            var nodeFetchCommand = Node.CreateCommandToFetchAll();
            nodeFetchCommand.Connection = _databaseConnection;

            using (var reader = new SafeDatabaseReader(nodeFetchCommand.ExecuteReader()))
                while (reader.Next())
                    _nodes.Add(new Node(reader, _databaseConnection));
        }

        #endregion

        #region public functions

        public IReadOnlyList<IReadOnlyNode> Nodes
        {
            get { return _nodes.Cast<IReadOnlyNode>().ToList(); }
        }

        #endregion
    }
}
