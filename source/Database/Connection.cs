using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Database
{
    public class Connection : INotifyPropertyChanged
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

        public void ToggleConnect()
        {
            if (Connected)
                Disconnect();
            else
                Connect();

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
            var createDatabaseCommand = new SqlCommand("CREATE DATABASE " + Database + ";", _sqlConnection);
            var selectDatabaseCommand = new SqlCommand("USE " + Database + ";", _sqlConnection);
            var createPowerNetTable = 
                new SqlCommand(
                    "CREATE TABLE powernets " +
                    "(PowerNetId INTEGER NOT NULL IDENTITY, Frequency REAL, PowerNetName TEXT, CalculatorSelection INTEGER, " +
                    "PRIMARY KEY(PowerNetId));", _sqlConnection);
            var createNodeTable = 
                new SqlCommand(
                    "CREATE TABLE nodes " +
                    "(NodeId INTEGER NOT NULL IDENTITY, PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), NodeName TEXT, NominalVoltage REAL, " +
                    "NodeVoltageReal REAL, NodeVoltageImaginary REAL, " +
                    "PRIMARY KEY(NodeId));", _sqlConnection);
            var createLoadTable = 
                new SqlCommand(
                    "CREATE TABLE loads " +
                    "(LoadId INTEGER NOT NULL IDENTITY, Node INTEGER NOT NULL REFERENCES nodes (NodeId), LoadName TEXT, LoadReal REAL, LoadImaginary REAL, " +
                    "PRIMARY KEY(LoadId));", _sqlConnection);
            var createFeedInTable = 
                new SqlCommand(
                    "CREATE TABLE feedins " +
                    "(FeedInId INTEGER NOT NULL IDENTITY, Node INTEGER NOT NULL REFERENCES nodes (NodeId), FeedInName TEXT, VoltageReal REAL, VoltageImaginary REAL, ShortCircuitPower REAL, " +
                    "PRIMARY KEY(FeedInId));", _sqlConnection);
            var createGeneratorTable = 
                new SqlCommand(
                    "CREATE TABLE generators " +
                    "(GeneratorId INTEGER NOT NULL IDENTITY, Node INTEGER NOT NULL REFERENCES nodes (NodeId), GeneratorName TEXT, VoltageMagnitude REAL, RealPower REAL, " +
                    "PRIMARY KEY(GeneratorId));", _sqlConnection);
            var createTransformerTable = 
                new SqlCommand(
                    "CREATE TABLE transformers " +
                    "(TransformerId INTEGER NOT NULL IDENTITY, UpperSideNode INTEGER NOT NULL REFERENCES nodes (NodeId), LowerSideNode INTEGER NOT NULL REFERENCES nodes (NodeId), " +
                    "TransformerName TEXT, NominalPower REAL, RelativeShortCircuitVoltage REAL, CopperLosses REAL, IronLosses REAL, RelativeNoLoadCurrent REAL, Ratio REAL, " +
                    "PRIMARY KEY(TransformerId));", _sqlConnection);
            var createLineTable = 
                new SqlCommand(
                    "CREATE TABLE lines " +
                    "(LineId INTEGER NOT NULL IDENTITY, NodeOne INTEGER NOT NULL REFERENCES nodes (NodeId), NodeTwo INTEGER NOT NULL REFERENCES nodes (NodeId), " +
                    "LineName TEXT, SeriesResistancePerUnitLength REAL, SeriesInductancePerUnitLength REAL, ShuntConductancePerUnitLength REAL, " +
                    "ShuntCapacityPerUnitLength REAL, Length REAL, " +
                    "PRIMARY KEY(LineId));", _sqlConnection);

            createDatabaseCommand.ExecuteNonQuery();
            selectDatabaseCommand.ExecuteNonQuery();
            createPowerNetTable.ExecuteNonQuery();
            createNodeTable.ExecuteNonQuery();
            createLoadTable.ExecuteNonQuery();
            createFeedInTable.ExecuteNonQuery();
            createGeneratorTable.ExecuteNonQuery();
            createTransformerTable.ExecuteNonQuery();
            createLineTable.ExecuteNonQuery();

            Disconnect();
        }

        public void Add(PowerNet powerNet)
        {
            var nameParam = new SqlParameter("Name", SqlDbType.Text) { Value = powerNet.Name };
            var frequencyParam = new SqlParameter("Frequency", SqlDbType.Real) { Value = powerNet.Frequency };
            var calculatorSelectionParam = new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = powerNet.CalculatorSelection };
            var command =
                new SqlCommand(
                    "INSERT INTO powernets (PowerNetName, Frequency, CalculatorSelection) OUTPUT INSERTED.PowerNetId VALUES(@Name, @Frequency, @CalculatorSelection);",
                    _sqlConnection);
            command.Parameters.Add(nameParam);
            command.Parameters.Add(frequencyParam);
            command.Parameters.Add(calculatorSelectionParam);
            powerNet.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Update(PowerNet powerNet)
        {
            var nameParam = new SqlParameter("Name", SqlDbType.Text) { Value = powerNet.Name };
            var frequencyParam = new SqlParameter("Frequency", SqlDbType.Real) { Value = powerNet.Frequency };
            var calculatorSelectionParam = new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = powerNet.CalculatorSelection };
            var idParam = new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id };
            var command =
                new SqlCommand(
                    "UPDATE powernets SET PowerNetName=@Name, Frequency=@Frequency, CalculatorSelection=@CalculatorSelection WHERE PowerNetId=@Id;",
                    _sqlConnection);
            command.Parameters.Add(nameParam);
            command.Parameters.Add(frequencyParam);
            command.Parameters.Add(calculatorSelectionParam);
            command.Parameters.Add(idParam);
            command.ExecuteNonQuery();
        }

        public void Remove(PowerNet powerNet)
        {
            var idParam = new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id };
            var command = new SqlCommand("DELETE FROM powernets WHERE PowerNetId=@Id;", _sqlConnection);
            command.Parameters.Add(idParam);

            using (var transaction = _sqlConnection.BeginTransaction())
            {
                try
                {
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception)
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
            var reader = command.ExecuteReader();

            while (reader.Read())
                powerNets.Add(ReadPowerNetFromRecord(reader));

            reader.Close();
            ReadNetElements(powerNets);
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

        private void Connect()
        {
            _sqlConnection = new SqlConnection(ConnectionString);

            try
            {
                _sqlConnection.Open();
            }
            catch (Exception e)
            {
                _sqlConnection.Dispose();
                _sqlConnection = null;
                throw;
            }
        }

        private void Disconnect()
        {
            _sqlConnection.Close();
            _sqlConnection.Dispose();
            _sqlConnection = null;
        }

        private void NotifyConnectedChanged()
        {
            NotifyPropertyChangedInternal("Connected");
            NotifyPropertyChangedInternal("NotConnected");
        }

        private IReadOnlyDictionary<int, Node> ReadNodes(PowerNet powerNet)
        {
            powerNet.Nodes.Clear();
            var nodeIds = new Dictionary<int, Node>();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = new SqlCommand("SELECT * FROM nodes WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var node = ReadNodeFromRecord(reader);
                nodeIds.Add(node.Id, node);
                powerNet.Nodes.Add(node);
            }

            reader.Close();
            return nodeIds;
        }

        private void ReadNetElements(IEnumerable<PowerNet> powerNets)
        {
            foreach (var powerNet in powerNets)
            {
                var nodeIds = ReadNodes(powerNet);
                ReadLoads(powerNet, nodeIds);
                ReadLines(powerNet, nodeIds);
                ReadFeedIns(powerNet, nodeIds);
            }
        }

        private void ReadLoads(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Loads.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = new SqlCommand("SELECT LoadId, NodeId, LoadName, LoadReal, LoadImaginary FROM loads INNER JOIN nodes ON Node=NodeId WHERE nodes.PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var load = ReadLoadFromRecord(nodeIds, reader);
                powerNet.Loads.Add(load);
            }

            reader.Close();
        }

        private void ReadLines(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Lines.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = 
                new SqlCommand(
                    "SELECT " +
                    "LineId, NodeOne, NodeTwo, LineName, SeriesResistancePerUnitLength, SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength, Length " +
                    "FROM lines INNER JOIN nodes ON NodeOne=NodeId WHERE nodes.PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var line = ReadLineFromRecord(nodeIds, reader);
                powerNet.Lines.Add(line);
            }

            reader.Close();
        }

        private void ReadFeedIns(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.FeedIns.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = 
                new SqlCommand(
                    "SELECT FeedInId, NodeId, FeedInName, VoltageReal, VoltageImaginary, ShortCircuitPower " +
                    "FROM feedins INNER JOIN nodes ON Node=NodeId WHERE nodes.PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var feedIn = ReadFeedInFromRecord(nodeIds, reader);
                powerNet.FeedIns.Add(feedIn);
            }

            reader.Close();
        }

        #endregion

        #region reading from single records

        private static FeedIn ReadFeedInFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var nodeId = Convert.ToInt32(reader["NodeId"].ToString());
            var node = nodeIds[nodeId];
            return new FeedIn
            {
                Id = Convert.ToInt32(reader["FeedInId"].ToString()),
                Name = reader["FeedInName"].ToString(),
                Node = node,
                VoltageReal = Convert.ToDouble(reader["VoltageReal"].ToString()),
                VoltageImaginary = Convert.ToDouble(reader["VoltageImaginary"].ToString()),
                ShortCircuitPower = Convert.ToDouble(reader["ShortCircuitPower"].ToString())
            };
        }

        private static Line ReadLineFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var nodeOneId = Convert.ToInt32(reader["NodeOne"].ToString());
            var nodeOne = nodeIds[nodeOneId];
            var nodeTwoId = Convert.ToInt32(reader["NodeTwo"].ToString());
            var nodeTwo = nodeIds[nodeTwoId];
            return new Line
            {
                Id = Convert.ToInt32(reader["LineId"].ToString()),
                Name = reader["LineName"].ToString(),
                SeriesResistancePerUnitLength = Convert.ToDouble(reader["SeriesResistancePerUnitLength"].ToString()),
                SeriesInductancePerUnitLength = Convert.ToDouble(reader["SeriesInductancePerUnitLength"].ToString()),
                ShuntConductancePerUnitLength = Convert.ToDouble(reader["ShuntConductancePerUnitLength"].ToString()),
                ShuntCapacityPerUnitLength = Convert.ToDouble(reader["ShuntCapacityPerUnitLength"].ToString()),
                Length = Convert.ToDouble(reader["Length"].ToString()),
                NodeOne = nodeOne,
                NodeTwo = nodeTwo
            };
        }

        private static PowerNet ReadPowerNetFromRecord(IDataRecord record)
        {
            var powerNet = new PowerNet
            {
                Id = Convert.ToInt32(record["PowerNetId"].ToString()),
                Name = record["PowerNetName"].ToString(),
                Frequency = Convert.ToDouble(record["Frequency"]),
                CalculatorSelection =
                    (NodeVoltageCalculatorSelection)Convert.ToInt32(record["CalculatorSelection"].ToString())
            };

            return powerNet;
        }

        private static Node ReadNodeFromRecord(IDataRecord reader)
        {
            return new Node
            {
                Id = Convert.ToInt32(reader["NodeId"].ToString()),
                Name = reader["NodeName"].ToString(),
                NominalVoltage = Convert.ToDouble(reader["NominalVoltage"].ToString()),
                VoltageReal = Convert.ToDouble(reader["NodeVoltageReal"].ToString()),
                VoltageImaginary = Convert.ToDouble(reader["NodeVoltageImaginary"].ToString())
            };
        }

        private static Load ReadLoadFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var nodeId = Convert.ToInt32(reader["NodeId"].ToString());
            var node = nodeIds[nodeId];
            return new Load
            {
                Id = Convert.ToInt32(reader["LoadId"].ToString()),
                Name = reader["LoadName"].ToString(),
                Real = Convert.ToDouble(reader["LoadReal"].ToString()),
                Imaginary = Convert.ToDouble(reader["LoadImaginary"].ToString()),
                Node = node
            };
        }

        #endregion
    }
}
