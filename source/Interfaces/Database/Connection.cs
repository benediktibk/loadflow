using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Misc;

namespace Database
{
    public class Connection : 
        INotifyPropertyChanged, IConnectionNetElements, IDisposable
    {
        private string _server;
        private string _user;
        private string _password;
        private string _database;
        private SqlConnection _sqlConnection;

        public Connection()
        {
            Server = ".\\SQLEXPRESS";
            Database = "loadflow";
        }

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
                SqlCommandFactory.CreateCommandToCreateTableForPowerNets(),
                SqlCommandFactory.CreateCommandToCreateNodeTable(),
                SqlCommandFactory.CreateCommandToCreateLoadTable(),
                SqlCommandFactory.CreateCommandToCreateFeedInTable(),
                SqlCommandFactory.CreateCommandToCreateGeneratorTable(),
                SqlCommandFactory.CreateCommandToCreateTransformerTable(),
                SqlCommandFactory.CreateCommandToCreateTransmissionLineTable()
            };
            commands.AddRange(SqlCommandFactory.CreateCommandsToCreateAdmittanceMatrixTables());

            foreach (var command in commands)
            {
                command.Connection = _sqlConnection;
                command.ExecuteNonQuery();
            }

            Disconnect();
        }

        public void Add(PowerNet powerNet)
        {
            var command = SqlCommandFactory.CreateCommandToAddToDatabase(powerNet);
            command.Connection = _sqlConnection;
            powerNet.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Update(IReadOnlyPowerNet powerNet)
        {
            var command = SqlCommandFactory.CreateCommandToUpdateInDatabase(powerNet);
            command.Connection = _sqlConnection;
            command.ExecuteNonQuery();
        }

        public void Remove(IReadOnlyPowerNet powerNet)
        {
            var commands = SqlCommandFactory.CreateCommandsToRemoveFromDatabase(powerNet);

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

        public void ReadPowerNets(ObservableCollection<PowerNetComputable> powerNets)
        {
            powerNets.Clear();
            var command = new SqlCommand("SELECT * FROM powernets;", _sqlConnection);

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNets.Add(new PowerNetComputable(reader, this));

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
            var headerCommand = SqlCommandFactory.CreateCommandToAddAdmittanceMatrixHeader(matrix, powerNet, powerBase);
            headerCommand.Connection = _sqlConnection;
            var matrixId = Convert.ToInt32(headerCommand.ExecuteScalar().ToString());
            var contentCommands = SqlCommandFactory.CreateCommandsToAddContent(matrix, nodeNames, matrixId);

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

        private void NotifyConnectedChanged()
        {
            NotifyPropertyChangedInternal("Connected");
            NotifyPropertyChangedInternal("NotConnected");
        }

        private IReadOnlyDictionary<int, Node> ReadNodes(PowerNet powerNet)
        {
            powerNet.Nodes.Clear();
            var nodeIds = new Dictionary<int, Node> {{0, null}};
            var command = SqlCommandFactory.CreateCommandToFetchAllNodes(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
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
            var command = SqlCommandFactory.CreateCommandToFetchAllLoads(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Loads.Add(new Load(nodeIds, reader));
        }

        private void ReadLines(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.TransmissionLines.Clear();
            var command = SqlCommandFactory.CreateCommandToFetchAllTransmssionLines(powerNet.Id);
            command.Connection = _sqlConnection;

            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.TransmissionLines.Add(new TransmissionLine(nodeIds, reader));
        }

        private void ReadFeedIns(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.FeedIns.Clear();
            var command = SqlCommandFactory.CreateCommandToFetchAllFeedIns(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.FeedIns.Add(new FeedIn(nodeIds, reader));
        }

        private void ReadGenerators(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Generators.Clear();
            var command = SqlCommandFactory.CreateCommandToFetchAllGenerators(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Generators.Add(new Generator(nodeIds, reader));
        }

        private void ReadTransformers(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Transformers.Clear();
            var command = SqlCommandFactory.CreateCommandToFetchAllTransformers(powerNet.Id);
            command.Connection = _sqlConnection;
            
            using (var reader = new SafeDatabaseReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Transformers.Add(new Transformer(nodeIds, reader));
        }

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
    }
}
