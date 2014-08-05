using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Database
{
    public class Connection : INotifyPropertyChanged, IConnectionNetElements, IDisposable
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
            var createDatabaseCommand = new SqlCommand("CREATE DATABASE " + Database + ";", _sqlConnection);
            var selectDatabaseCommand = new SqlCommand("USE " + Database + ";", _sqlConnection);
            var createPowerNetTable = 
                new SqlCommand(
                    "CREATE TABLE powernets " +
                    "(PowerNetId INTEGER NOT NULL IDENTITY, Frequency REAL NOT NULL, PowerNetName TEXT NOT NULL, CalculatorSelection INTEGER NOT NULL, " +
                    "PRIMARY KEY(PowerNetId));", _sqlConnection);
            var createNodeTable = 
                new SqlCommand(
                    "CREATE TABLE nodes " +
                    "(NodeId INTEGER NOT NULL IDENTITY, PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), NodeName TEXT NOT NULL, NominalVoltage REAL NOT NULL, " +
                    "NodeVoltageReal REAL NOT NULL, NodeVoltageImaginary REAL NOT NULL, " +
                    "PRIMARY KEY(NodeId));", _sqlConnection);
            var createLoadTable = 
                new SqlCommand(
                    "CREATE TABLE loads " +
                    "(LoadId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                    "LoadName TEXT NOT NULL, LoadReal REAL NOT NULL, LoadImaginary REAL NOT NULL, " +
                    "PRIMARY KEY(LoadId));", _sqlConnection);
            var createFeedInTable = 
                new SqlCommand(
                    "CREATE TABLE feedins " +
                    "(FeedInId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                    "FeedInName TEXT NOT NULL, VoltageReal REAL NOT NULL, VoltageImaginary REAL NOT NULL, ShortCircuitPower REAL NOT NULL, C REAL NOT NULL, RealToImaginary REAL NOT NULL, " +
                    "PRIMARY KEY(FeedInId));", _sqlConnection);
            var createGeneratorTable = 
                new SqlCommand(
                    "CREATE TABLE generators " +
                    "(GeneratorId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                    "GeneratorName TEXT NOT NULL, VoltageMagnitude REAL NOT NULL, RealPower REAL NOT NULL, " +
                    "PRIMARY KEY(GeneratorId));", _sqlConnection);
            var createTransformerTable = 
                new SqlCommand(
                    "CREATE TABLE transformers " +
                    "(TransformerId INTEGER NOT NULL IDENTITY, UpperSideNode INTEGER REFERENCES nodes (NodeId), LowerSideNode INTEGER REFERENCES nodes (NodeId), " +
                    "PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), TransformerName TEXT NOT NULL, NominalPower REAL NOT NULL, " +
                    "RelativeShortCircuitVoltage REAL NOT NULL, CopperLosses REAL NOT NULL, IronLosses REAL NOT NULL, RelativeNoLoadCurrent REAL NOT NULL, Ratio REAL NOT NULL, " +
                    "PRIMARY KEY(TransformerId));", _sqlConnection);
            var createLineTable = 
                new SqlCommand(
                    "CREATE TABLE lines " +
                    "(LineId INTEGER NOT NULL IDENTITY, NodeOne INTEGER REFERENCES nodes (NodeId), NodeTwo INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                    "LineName TEXT NOT NULL, SeriesResistancePerUnitLength REAL NOT NULL, SeriesInductancePerUnitLength REAL NOT NULL, ShuntConductancePerUnitLength REAL NOT NULL, " +
                    "ShuntCapacityPerUnitLength REAL NOT NULL, Length REAL NOT NULL, " +
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
                    powerNets.Add(ReadPowerNetFromRecord(reader));

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
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = new SqlCommand("SELECT * FROM nodes WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                {
                    var node = ReadNodeFromRecord(reader);
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
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = new SqlCommand("SELECT LoadId, Node, LoadName, LoadReal, LoadImaginary FROM loads WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Loads.Add(ReadLoadFromRecord(nodeIds, reader));
        }

        private void ReadLines(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Lines.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = 
                new SqlCommand(
                    "SELECT " +
                    "LineId, NodeOne, NodeTwo, LineName, SeriesResistancePerUnitLength, SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength, Length " +
                    "FROM lines WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);

            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Lines.Add(ReadLineFromRecord(nodeIds, reader));
        }

        private void ReadFeedIns(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.FeedIns.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command = 
                new SqlCommand(
                    "SELECT FeedInId, Node, FeedInName, VoltageReal, VoltageImaginary, ShortCircuitPower, C, RealToImaginary " +
                    "FROM feedins WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.FeedIns.Add(ReadFeedInFromRecord(nodeIds, reader));
        }

        private void ReadGenerators(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Generators.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command =
                new SqlCommand(
                    "SELECT GeneratorId, Node, GeneratorName, VoltageMagnitude, RealPower " +
                    "FROM generators WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Generators.Add(ReadGeneratorFromRecord(nodeIds, reader));
        }

        private void ReadTransformers(PowerNet powerNet, IReadOnlyDictionary<int, Node> nodeIds)
        {
            powerNet.Transformers.Clear();
            var powerNetParam = new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id };
            var command =
                new SqlCommand(
                    "SELECT TransformerId, UpperSideNode, LowerSideNode, TransformerName, NominalPower, RelativeShortCircuitVoltage, CopperLosses, IronLosses, RelativeNoLoadCurrent, Ratio " +
                    "FROM transformers WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            
            using (var reader = new SafeSqlDataReader(command.ExecuteReader()))
                while (reader.Next())
                    powerNet.Transformers.Add(ReadTransformerFromRecord(nodeIds, reader));
        }

        #endregion

        #region reading from single records

        private static FeedIn ReadFeedInFromRecord(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            return new FeedIn
            {
                Id = reader.Parse<int>("FeedInId"),
                Name = reader.Parse<string>("FeedInName"),
                Node = node,
                VoltageReal = reader.Parse<double>("VoltageReal"),
                VoltageImaginary = reader.Parse<double>("VoltageImaginary"),
                ShortCircuitPower = reader.Parse<double>("ShortCircuitPower"),
                C = reader.Parse<double>("C"),
                RealToImaginary = reader.Parse<double>("RealToImaginary")
            };
        }

        private static Line ReadLineFromRecord(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var nodeOneId = reader.Parse<int>("NodeOne");
            var nodeOne = nodeIds[nodeOneId];
            var nodeTwoId = reader.Parse<int>("NodeTwo");
            var nodeTwo = nodeIds[nodeTwoId];
            return new Line
            {
                Id = reader.Parse<int>("LineId"),
                Name = reader.Parse<string>("LineName"),
                SeriesResistancePerUnitLength = reader.Parse<double>("SeriesResistancePerUnitLength"),
                SeriesInductancePerUnitLength = reader.Parse<double>("SeriesInductancePerUnitLength"),
                ShuntConductancePerUnitLength = reader.Parse<double>("ShuntConductancePerUnitLength"),
                ShuntCapacityPerUnitLength = reader.Parse<double>("ShuntCapacityPerUnitLength"),
                Length = reader.Parse<double>("Length"),
                NodeOne = nodeOne,
                NodeTwo = nodeTwo
            };
        }

        private PowerNet ReadPowerNetFromRecord(ISafeDataRecord reader)
        {
            var powerNet = new PowerNet
            {
                Id = reader.Parse<int>("PowerNetId"),
                Name = reader.Parse<string>("PowerNetName"),
                Frequency = reader.Parse<double>("Frequency"),
                CalculatorSelection =
                    (NodeVoltageCalculatorSelection)reader.Parse<int>("CalculatorSelection"),
                Connection = this
            };

            return powerNet;
        }

        private static Node ReadNodeFromRecord(ISafeDataRecord reader)
        {
            return new Node
            {
                Id = reader.Parse<int>("NodeId"),
                Name = reader.Parse<string>("NodeName"),
                NominalVoltage = reader.Parse<double>("NominalVoltage"),
                VoltageReal = reader.Parse<double>("NodeVoltageReal"),
                VoltageImaginary = reader.Parse<double>("NodeVoltageImaginary")
            };
        }

        private static Load ReadLoadFromRecord(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            return new Load
            {
                Id = reader.Parse<int>("LoadId"),
                Name = reader.Parse<string>("LoadName"),
                Real = reader.Parse<double>("LoadReal"),
                Imaginary = reader.Parse<double>("LoadImaginary"),
                Node = node
            };
        }

        private static Generator ReadGeneratorFromRecord(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            return new Generator
            {
                Id = reader.Parse<int>("GeneratorId"),
                Name = reader.Parse<string>("GeneratorName"),
                VoltageMagnitude = reader.Parse<double>("VoltageMagnitude"),
                RealPower = reader.Parse<double>("RealPower"),
                Node = node
            };
        }

        private static Transformer ReadTransformerFromRecord(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var upperSideNodeId = reader.Parse<int>("UpperSideNode");
            var upperSideNode = nodeIds[upperSideNodeId];
            var lowerSideNodeId = reader.Parse<int>("LowerSideNode");
            var lowerSideNode = nodeIds[lowerSideNodeId];
            return new Transformer
            {
                Id = reader.Parse<int>("TransformerId"),
                Name = reader.Parse<string>("TransformerName"),
                NominalPower = reader.Parse<double>("NominalPower"),
                RelativeShortCircuitVoltage = reader.Parse<double>("RelativeShortCircuitVoltage"),
                CopperLosses = reader.Parse<double>("CopperLosses"),
                IronLosses = reader.Parse<double>("IronLosses"),
                RelativeNoLoadCurrent = reader.Parse<double>("RelativeNoLoadCurrent"),
                Ratio = reader.Parse<double>("Ratio"),
                UpperSideNode =  upperSideNode,
                LowerSideNode = lowerSideNode
            };
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
