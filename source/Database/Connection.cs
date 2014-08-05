using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Database
{
    public class Connection : 
        INotifyPropertyChanged, IConnectionNetElements, IDisposable
    {
        #region variables

        private string _server;
        private string _user;
        private string _password;
        private string _database;
        private SqlConnection _sqlConnection;

        #endregion

        #region constructor

        public Connection()
        {
            Server = ".\\SQLEXPRESS";
            Database = "loadflow";
        }

        #endregion

        #region public functions

        public void Connect()
        {
            _sqlConnection = new SqlConnection(ConnectionString);

            try
            {
                _sqlConnection.Open();
            }
            catch (Exception)
            {
                _sqlConnection.Dispose();
                _sqlConnection = null;
                throw;
            }

            NotifyConnectedChanged();
        }

        public void Disconnect()
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = null;
            NotifyConnectedChanged();
        }

        public void CreateDatabase()
        {
            if (Connected)
                throw new InvalidOperationException("can not create database if the connection is already established");

            if (!Regex.IsMatch(Database, @"^[a-zA-Z0-9_]+$"))
                throw new ArgumentException("the database name must contain only letters, numbers and underscores");

            _sqlConnection = new SqlConnection(ConnectionStringWithoutDatabase);
            _sqlConnection.Open();
            var commands = new List<SqlCommand>
            {
                new SqlCommand("CREATE DATABASE " + Database + ";"),
                new SqlCommand("USE " + Database + ";"),
                PowerNet.CreateCommandToCreateTable(),
                Node.CreateCommandToCreateTable(),
                Load.CreateCommandToCreateTable(),
                FeedIn.CreateCommandToCreateTable(),
                Generator.CreateCommandToCreateTable(),
                Transformer.CreateCommandToCreateTable(),
                Line.CreateCommandToCreateTable()
            };
            commands.AddRange(AdmittanceMatrix.CreateCommandsToCreateTables());

            foreach (var command in commands)
            {
                command.Connection = _sqlConnection;
                command.ExecuteNonQuery();
            }

            Disconnect();
        }

        public void Add(PowerNet powerNet)
        {
            var command = powerNet.CreateCommandToAddToDatabase();
            command.Connection = _sqlConnection;
            powerNet.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Update(PowerNet powerNet)
        {
            var command = powerNet.CreateCommandToUpdateInDatabase();
            command.Connection = _sqlConnection;
            command.ExecuteNonQuery();
        }

        public void Remove(PowerNet powerNet)
        {
            var commands = powerNet.CreateCommandsToRemoveFromDatabase();

            using (var transaction = _sqlConnection.BeginTransaction())
            {
                foreach (var command in commands)
                {
                    command.Connection = _sqlConnection;
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }

                try
                {
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void ReadPowerNets(ObservableCollection<PowerNet> powerNets)
        {
            powerNets.Clear();
            var command = new SqlCommand("SELECT * FROM powernets;", _sqlConnection);

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNets.Add(new PowerNet(reader, this));

            ReadNetElements(powerNets);
        }

        public void Dispose()
        {
            if (_sqlConnection == null)
                return;

            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        public void Add(Calculation.SinglePhase.MultipleVoltageLevels.IAdmittanceMatrix matrix,
            IReadOnlyList<string> nodeNames, string powerNet, double powerBase)
        {
            var headerCommand = AdmittanceMatrix.CreateCommandToAddHeader(matrix, powerNet, powerBase);
            headerCommand.Connection = _sqlConnection;
            var matrixId = Convert.ToInt32(headerCommand.ExecuteScalar().ToString());
            var contentCommands = AdmittanceMatrix.CreateCommandsToAddContent(matrix, nodeNames, matrixId);

            using (var transaction = _sqlConnection.BeginTransaction())
            {
                try
                {
                    foreach (var command in contentCommands)
                    {
                        command.Connection = _sqlConnection;
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region properties

        public string Server
        {
            get { return _server; }
            set
            {
                if (_server == value) return;

                _server = value;
                NotifyPropertyChanged();
            }
        }

        public string User
        {
            get { return _user; }
            set
            {
                if (_user == value) return;

                _user = value;
                NotifyPropertyChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value) return;

                _password = value;
                NotifyPropertyChanged();
            }
        }

        public string Database
        {
            get { return _database; }
            set
            {
                if (_database == value) return;

                _database = value;
                NotifyPropertyChanged();
            }
        }

        public bool Connected
        {
            get { return _sqlConnection != null; }
        }

        public bool NotConnected
        {
            get { return !Connected; }
        }

        private string ConnectionString
        {
            get
            { return ConnectionStringWithoutDatabase + ";" + "database=" + Database; }
        }

        private string ConnectionStringWithoutDatabase
        {
            get
            {
                var connectionString = "";

                if (!string.IsNullOrEmpty(User))
                {
                    connectionString += "user id=" + User + ";";
                    connectionString += "password=" + Password + ";";
                }
                else
                    connectionString += "Trusted_Connection=True;";

                connectionString += "server=" + Server + ";";
                connectionString += "connection timeout=5";
                return connectionString;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            NotifyPropertyChangedInternal(propertyName);
        }

        private void NotifyPropertyChangedInternal(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region private functions

        private void NotifyConnectedChanged()
        {
            NotifyPropertyChangedInternal("Connected");
            NotifyPropertyChangedInternal("NotConnected");
        }

        private IReadOnlyDictionary<int, Node> ReadNodes(PowerNet powerNet)
        {
            powerNet.Nodes.Clear();
            var nodeIds = new Dictionary<int, Node> {{0, null}};
            var command = Node.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                {
                    var node = new Node(reader);
                    nodeIds.Add(node.Id, node);
                    powerNet.Nodes.Add(node);
                }

            return nodeIds;
        }

        private void ReadNetElements(IEnumerable<PowerNet> powerNets)
        {
            foreach (var powerNet in powerNets)
            {
                powerNet.ReactToChangesWithDatabaseUpdate = false;
                var nodeIds = ReadNodes(powerNet);
                ReadLoads(powerNet, nodeIds);
                ReadLines(powerNet, nodeIds);
                ReadFeedIns(powerNet, nodeIds);
                ReadGenerators(powerNet, nodeIds);
                ReadTransformers(powerNet, nodeIds);
                powerNet.ReactToChangesWithDatabaseUpdate = true;
            }
        }

        private void ReadLoads(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Loads.Clear();
            var command = Load.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Loads.Add(new Load(nodeIds, reader));
        }

        private void ReadLines(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Lines.Clear();
            var command = Line.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Lines.Add(new Line(nodeIds, reader));
        }

        private void ReadFeedIns(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.FeedIns.Clear();
            var command = FeedIn.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.FeedIns.Add(new FeedIn(nodeIds, reader));
        }

        private void ReadGenerators(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Generators.Clear();
            var command = Generator.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Generators.Add(new Generator(nodeIds, reader));
        }

        private void ReadTransformers(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Transformers.Clear();
            var command = Transformer.CreateCommandToFetchAll(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Transformers.Add(new Transformer(nodeIds, reader));
        }

        #endregion

        #region IConnectionNetElements

        public void Add(INetElement element, int powerNetId)
        {
            var command = element.CreateCommandToAddToDatabase(powerNetId);
            command.Connection = _sqlConnection;
            element.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void AddList(IList elements, int powerNetId)
        {
            if (elements == null)
                return;

            var elementsCasted = elements.Cast<INetElement>();
            foreach (var element in elementsCasted)
                Add(element, powerNetId);
        }

        public void Update(INetElement element)
        {
            var command = element.CreateCommandToUpdateInDatabase();
            command.Connection = _sqlConnection;
            command.ExecuteNonQuery();
        }

        public void Remove(INetElement element)
        {
            var command = element.CreateCommandToRemoveFromDatabase();
            command.Connection = _sqlConnection;
            command.ExecuteNonQuery();
        }

        public void RemoveList(IList elements)
        {
            if (elements == null)
                return;

            var elementsCasted = elements.Cast<INetElement>();
            foreach (var element in elementsCasted)
                Remove(element);
        }

        #endregion
    }
}
