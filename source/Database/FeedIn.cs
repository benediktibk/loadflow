using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using DatabaseHelper;

namespace Database
{
    public class FeedIn : INetElement
    {
        #region variables

        private string _name;
        private Node _node;
        private double _voltageReal;
        private double _voltageImaginary;
        private double _shortCircuitPower;
        private double _c;
        private double _realToImaginary;

        #endregion

        #region constructors

        public FeedIn()
        {
            VoltageReal = 1;
            VoltageImaginary = 0;
            Name = "";
            C = 1.1;
            RealToImaginary = 0.1;

        }

        public FeedIn(IReadOnlyDictionary<int, Node> nodeIds, ISafeDatabaseRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            Id = reader.Parse<int>("FeedInId");
            Name = reader.Parse<string>("FeedInName");
            Node = node;
            VoltageReal = reader.Parse<double>("VoltageReal");
            VoltageImaginary = reader.Parse<double>("VoltageImaginary");
            ShortCircuitPower = reader.Parse<double>("ShortCircuitPower");
            C = reader.Parse<double>("C");
            RealToImaginary = reader.Parse<double>("RealToImaginary");
        }

        #endregion

        #region properties

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;

                _name = value;
                NotifyPropertyChanged();
            }
        }

        public Node Node
        {
            get { return _node; }
            set
            {
                if (_node == value) return;

                _node = value;
                NotifyPropertyChanged();
            }
        }

        public double VoltageReal
        {
            get { return _voltageReal; }
            set
            {
                if (_voltageReal == value) return;

                _voltageReal = value;
                NotifyPropertyChanged();
            }
        }

        public double VoltageImaginary
        {
            get { return _voltageImaginary; }
            set
            {
                if (_voltageImaginary == value) return;

                _voltageImaginary = value;
                NotifyPropertyChanged();
            }
        }

        public double ShortCircuitPower
        {
            get { return _shortCircuitPower; }
            set
            {
                if (_shortCircuitPower == value) return;

                _shortCircuitPower = value;
                NotifyPropertyChanged();
            }
        }

        public double C
        {
            get { return _c; }
            set
            {
                if (_c == value) return;

                _c = value;
                NotifyPropertyChanged();
            }
        }

        public double RealToImaginary
        {
            get { return _realToImaginary; }
            set
            {
                if (_realToImaginary == value) return;

                _realToImaginary = value;
                NotifyPropertyChanged();
            }
        }

        public object NodeForeignKey
        {
            get
            {
                if (Node == null)
                    return DBNull.Value;

                return Node.Id;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INetElement

        public SqlCommand CreateCommandToAddToDatabase(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "INSERT INTO feedins " +
                    "(FeedInName, PowerNet, VoltageReal, VoltageImaginary, ShortCircuitPower, C, RealToImaginary) " +
                    "OUTPUT INSERTED.FeedInId " +
                    "VALUES(@Name, @PowerNet, @VoltageReal, @VoltageImaginary, @ShortCircuitPower, @C, @RealToImaginary);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) { Value = VoltageImaginary });
            command.Parameters.Add(new SqlParameter("ShortCircuitPower", SqlDbType.Real) { Value = ShortCircuitPower });
            command.Parameters.Add(new SqlParameter("C", SqlDbType.Real) { Value = C });
            command.Parameters.Add(new SqlParameter("RealToImaginary", SqlDbType.Real) { Value = RealToImaginary });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE feedins SET " +
                    "Node=@Node, FeedInName=@Name, VoltageReal=@VoltageReal, VoltageImaginary=@VoltageImaginary, ShortCircuitPower=@ShortCircuitPower, C=@C, RealToImaginary=@RealToImaginary " +
                    "WHERE FeedInId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("Node", SqlDbType.Int) { Value = NodeForeignKey });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) { Value = VoltageImaginary });
            command.Parameters.Add(new SqlParameter("ShortCircuitPower", SqlDbType.Real) { Value = ShortCircuitPower });
            command.Parameters.Add(new SqlParameter("C", SqlDbType.Real) { Value = C });
            command.Parameters.Add(new SqlParameter("RealToImaginary", SqlDbType.Real) { Value = RealToImaginary });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM feedins WHERE FeedInId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            return command;
        }

        public bool UsesNode(Node node)
        {
            return node == Node;
        }

        #endregion

        #region static functions

        public static SqlCommand CreateCommandToCreateTable()
        {
            return new SqlCommand(
                "CREATE TABLE feedins " +
                "(FeedInId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "FeedInName TEXT NOT NULL, VoltageReal REAL NOT NULL, VoltageImaginary REAL NOT NULL, ShortCircuitPower REAL NOT NULL, C REAL NOT NULL, RealToImaginary REAL NOT NULL, " +
                "PRIMARY KEY(FeedInId));");
        }

        public static SqlCommand CreateCommandToFetchAll(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM feedins WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        #endregion
    }
}
