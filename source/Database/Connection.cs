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
    public class Connection : INotifyPropertyChanged, IConnectionNetElements
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
            catch (Exception e)
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
                    "FeedInName TEXT NOT NULL, VoltageReal REAL NOT NULL, VoltageImaginary REAL NOT NULL, ShortCircuitPower REAL NOT NULL, " +
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
            var deletePowerNetCommand = new SqlCommand("DELETE FROM powernets WHERE PowerNetId=@Id;", _sqlConnection);
            var deleteNodesCommand = new SqlCommand("DELETE FROM nodes WHERE PowerNet=@Id;", _sqlConnection);
            var deleteLoadsCommand = new SqlCommand("DELETE load FROM loads load INNER JOIN nodes node ON Node=NodeId WHERE PowerNet=@Id;", _sqlConnection);
            var deleteFeedInsCommand = new SqlCommand("DELETE feedin FROM feedins feedin INNER JOIN nodes node ON Node=NodeId WHERE PowerNet=@Id;", _sqlConnection);
            var deleteGeneratorsCommand = new SqlCommand("DELETE generator FROM generators generator INNER JOIN nodes node ON Node=NodeId WHERE PowerNet=@Id;", _sqlConnection);
            var deleteTransformersCommand = new SqlCommand("DELETE transformer FROM transformers transformer INNER JOIN nodes node ON UpperSideNode=NodeId WHERE PowerNet=@Id;", _sqlConnection);
            var deleteLinesCommand = new SqlCommand("DELETE line FROM lines line INNER JOIN nodes node ON NodeOne=NodeId WHERE PowerNet=@Id;", _sqlConnection);
            deletePowerNetCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteNodesCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteLoadsCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteFeedInsCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteGeneratorsCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteTransformersCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });
            deleteLinesCommand.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = powerNet.Id });

            using (var transaction = _sqlConnection.BeginTransaction())
            {
                deletePowerNetCommand.Transaction = transaction;
                deleteNodesCommand.Transaction = transaction;
                deleteLoadsCommand.Transaction = transaction;
                deleteFeedInsCommand.Transaction = transaction;
                deleteGeneratorsCommand.Transaction = transaction;
                deleteTransformersCommand.Transaction = transaction;
                deleteLinesCommand.Transaction = transaction;

                try
                {
                    deleteFeedInsCommand.ExecuteNonQuery();
                    deleteGeneratorsCommand.ExecuteNonQuery();
                    deleteTransformersCommand.ExecuteNonQuery();
                    deleteLinesCommand.ExecuteNonQuery();
                    deleteLoadsCommand.ExecuteNonQuery();
                    deleteNodesCommand.ExecuteNonQuery();
                    deletePowerNetCommand.ExecuteNonQuery();
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
                    "FROM lines WHERE PowerNet=@PowerNet;", _sqlConnection);
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
                    "SELECT FeedInId, Node, FeedInName, VoltageReal, VoltageImaginary, ShortCircuitPower " +
                    "FROM feedins WHERE PowerNet=@PowerNet;", _sqlConnection);
            command.Parameters.Add(powerNetParam);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var feedIn = ReadFeedInFromRecord(nodeIds, reader);
                powerNet.FeedIns.Add(feedIn);
            }

            reader.Close();
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
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var generator = ReadGeneratorFromRecord(nodeIds, reader);
                powerNet.Generators.Add(generator);
            }

            reader.Close();
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
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var transformer = ReadTransformerFromRecord(nodeIds, reader);
                powerNet.Transformers.Add(transformer);
            }

            reader.Close();
        }

        #endregion

        #region reading from single records

        private static FeedIn ReadFeedInFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var nodeId = Convert.ToInt32(reader["Node"].ToString());
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

        private PowerNet ReadPowerNetFromRecord(IDataRecord record)
        {
            var powerNet = new PowerNet
            {
                Id = Convert.ToInt32(record["PowerNetId"].ToString()),
                Name = record["PowerNetName"].ToString(),
                Frequency = Convert.ToDouble(record["Frequency"]),
                CalculatorSelection =
                    (NodeVoltageCalculatorSelection)Convert.ToInt32(record["CalculatorSelection"].ToString()),
                Connection = this
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
            var nodeId = Convert.ToInt32(reader["Node"].ToString());
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

        private static Generator ReadGeneratorFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var nodeId = Convert.ToInt32(reader["Node"].ToString());
            var node = nodeIds[nodeId];
            return new Generator
            {
                Id = Convert.ToInt32(reader["GeneratorId"].ToString()),
                Name = reader["GeneratorName"].ToString(),
                VoltageMagnitude = Convert.ToDouble(reader["VoltageMagnitude"].ToString()),
                RealPower = Convert.ToDouble(reader["RealPower"].ToString()),
                Node = node
            };
        }

        private static Transformer ReadTransformerFromRecord(IReadOnlyDictionary<int, Node> nodeIds, IDataRecord reader)
        {
            var upperSideNodeId = Convert.ToInt32(reader["UpperSideNode"].ToString());
            var upperSideNode = nodeIds[upperSideNodeId];
            var lowerSideNodeId = Convert.ToInt32(reader["LowerSideNode"].ToString());
            var lowerSideNode = nodeIds[lowerSideNodeId];
            return new Transformer
            {
                Id = Convert.ToInt32(reader["TransformerId"].ToString()),
                Name = reader["TransformerName"].ToString(),
                NominalPower = Convert.ToDouble(reader["NominalPower"].ToString()),
                RelativeShortCircuitVoltage = Convert.ToDouble(reader["RelativeShortCircuitVoltage"].ToString()),
                CopperLosses = Convert.ToDouble(reader["CopperLosses"].ToString()),
                IronLosses = Convert.ToDouble(reader["IronLosses"].ToString()),
                RelativeNoLoadCurrent = Convert.ToDouble(reader["RelativeNoLoadCurrent"].ToString()),
                Ratio = Convert.ToDouble(reader["Ratio"].ToString()),
                UpperSideNode =  upperSideNode,
                LowerSideNode = lowerSideNode
            };
        }

        #endregion

        #region IConnectionNetElements

        public void Add(Node node, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO nodes (NodeName, PowerNet, NominalVoltage, NodeVoltageReal, NodeVoltageImaginary) OUTPUT INSERTED.NodeId VALUES(@Name, @PowerNet, @NominalVoltage, @VoltageReal, @VoltageImaginary);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = node.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) {Value = powerNet.Id});
            command.Parameters.Add(new SqlParameter("NominalVoltage", SqlDbType.Real) { Value = node.NominalVoltage });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = node.VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) {Value = node.VoltageImaginary});
            node.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Add(FeedIn feedIn, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO feedins (FeedInName, PowerNet, VoltageReal, VoltageImaginary, ShortCircuitPower) OUTPUT INSERTED.FeedInId VALUES(@Name, @PowerNet, @VoltageReal, @VoltageImaginary, @ShortCircuitPower);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = feedIn.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = feedIn.VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) { Value = feedIn.VoltageImaginary });
            command.Parameters.Add(new SqlParameter("ShortCircuitPower", SqlDbType.Real) { Value = feedIn.ShortCircuitPower });
            feedIn.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Add(Generator generator, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO generators (GeneratorName, PowerNet, VoltageMagnitude, RealPower) OUTPUT INSERTED.GeneratorId VALUES(@Name, @PowerNet, @VoltageMagnitude, @RealPower);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = generator.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id });
            command.Parameters.Add(new SqlParameter("VoltageMagnitude", SqlDbType.Real) { Value = generator.VoltageMagnitude });
            command.Parameters.Add(new SqlParameter("RealPower", SqlDbType.Real) { Value = generator.RealPower });
            generator.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Add(Load load, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO loads (LoadName, PowerNet, LoadReal, LoadImaginary) OUTPUT INSERTED.LoadId VALUES(@Name, @PowerNet, @LoadReal, @LoadImaginary);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = load.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id });
            command.Parameters.Add(new SqlParameter("LoadReal", SqlDbType.Real) { Value = load.Real });
            command.Parameters.Add(new SqlParameter("LoadImaginary", SqlDbType.Real) { Value = load.Imaginary });
            load.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Add(Line line, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO lines (LineName, PowerNet, Length, SeriesResistancePerUnitLength, SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength) " +
                    "OUTPUT INSERTED.LineId " +
                    "VALUES(@Name, @PowerNet, @Length, @SeriesResistancePerUnitLength, @SeriesInductancePerUnitLength, @ShuntConductancePerUnitLength, @ShuntCapacityPerUnitLength);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = line.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id });
            command.Parameters.Add(new SqlParameter("Length", SqlDbType.Real) { Value = line.Length });
            command.Parameters.Add(new SqlParameter("SeriesResistancePerUnitLength", SqlDbType.Real) { Value = line.SeriesResistancePerUnitLength });
            command.Parameters.Add(new SqlParameter("SeriesInductancePerUnitLength", SqlDbType.Real) { Value = line.SeriesInductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntConductancePerUnitLength", SqlDbType.Real) { Value = line.ShuntConductancePerUnitLength });
            command.Parameters.Add(new SqlParameter("ShuntCapacityPerUnitLength", SqlDbType.Real) { Value = line.ShuntCapacityPerUnitLength });
            line.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Add(Transformer transformer, PowerNet powerNet)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO transformers (TransformerName, PowerNet, NominalPower, RelativeShortCircuitVoltage, CopperLosses, IronLosses, RelativeNoLoadCurrent, Ratio) " +
                    "OUTPUT INSERTED.TransformerId " +
                    "VALUES(@Name, @PowerNet, @NominalPower, @RelativeShortCircuitVoltage, @CopperLosses, @IronLosses, @RelativeNoLoadCurrent, @Ratio);",
                    _sqlConnection);
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = transformer.Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNet.Id });
            command.Parameters.Add(new SqlParameter("NominalPower", SqlDbType.Real) { Value = transformer.NominalPower });
            command.Parameters.Add(new SqlParameter("RelativeShortCircuitVoltage", SqlDbType.Real) { Value = transformer.RelativeShortCircuitVoltage });
            command.Parameters.Add(new SqlParameter("CopperLosses", SqlDbType.Real) { Value = transformer.CopperLosses });
            command.Parameters.Add(new SqlParameter("IronLosses", SqlDbType.Real) { Value = transformer.IronLosses });
            command.Parameters.Add(new SqlParameter("RelativeNoLoadCurrent", SqlDbType.Real) { Value = transformer.RelativeNoLoadCurrent });
            command.Parameters.Add(new SqlParameter("Ratio", SqlDbType.Real) { Value = transformer.Ratio });
            transformer.Id = Convert.ToInt32(command.ExecuteScalar().ToString());
        }

        public void Update(Node node)
        {
            throw new NotImplementedException();
        }

        public void Update(FeedIn feedIn)
        {
            throw new NotImplementedException();
        }

        public void Update(Generator generator)
        {
            throw new NotImplementedException();
        }

        public void Update(Load load)
        {
            throw new NotImplementedException();
        }

        public void Update(Line line)
        {
            throw new NotImplementedException();
        }

        public void Update(Transformer transformer)
        {
            throw new NotImplementedException();
        }

        public void Remove(Node node)
        {
            throw new NotImplementedException();
        }

        public void Remove(FeedIn feedIn)
        {
            throw new NotImplementedException();
        }

        public void Remove(Generator generator)
        {
            throw new NotImplementedException();
        }

        public void Remove(Load load)
        {
            throw new NotImplementedException();
        }

        public void Remove(Line line)
        {
            throw new NotImplementedException();
        }

        public void Remove(Transformer transformer)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
