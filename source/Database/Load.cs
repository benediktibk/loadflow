using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Load : INetElement
    {
        #region variables

        private string _name;
        private Node _node;
        private double _real;
        private double _imaginary;

        #endregion

        #region constructor

        public Load()
        {
            Real = 0;
            Imaginary = 0;
            Name = "";
        }

        public Load(IReadOnlyDictionary<int, Node> nodeIds, ISafeDataRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            Id = reader.Parse<int>("LoadId");
            Name = reader.Parse<string>("LoadName");
            Real = reader.Parse<double>("LoadReal");
            Imaginary = reader.Parse<double>("LoadImaginary");
            Node = node;
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

        public double Real
        {
            get { return _real; }
            set
            {
                if (_real == value) return;

                _real = value;
                NotifyPropertyChanged();
            }
        }

        public double Imaginary
        {
            get { return _imaginary; }
            set
            {
                if (_imaginary == value) return;

                _imaginary = value;
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
                    "INSERT INTO loads (LoadName, PowerNet, LoadReal, LoadImaginary) OUTPUT INSERTED.LoadId VALUES(@Name, @PowerNet, @LoadReal, @LoadImaginary);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("LoadReal", SqlDbType.Real) { Value = Real });
            command.Parameters.Add(new SqlParameter("LoadImaginary", SqlDbType.Real) { Value = Imaginary });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE loads SET Node=@Node, LoadName=@Name, LoadReal=@LoadReal, LoadImaginary=@LoadImaginary WHERE LoadId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("Node", SqlDbType.Int) { Value = NodeForeignKey });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("LoadReal", SqlDbType.Real) { Value = Real });
            command.Parameters.Add(new SqlParameter("LoadImaginary", SqlDbType.Real) { Value = Imaginary });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM loads WHERE LoadId=@Id;");
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
                "CREATE TABLE loads " +
                "(LoadId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "LoadName TEXT NOT NULL, LoadReal REAL NOT NULL, LoadImaginary REAL NOT NULL, " +
                "PRIMARY KEY(LoadId));");
        }

        public static SqlCommand CreateCommandToFetchAll(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM loads WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        #endregion
    }
}
