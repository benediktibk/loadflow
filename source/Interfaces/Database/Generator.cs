﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Misc;

namespace Database
{
    public class Generator : INetElement
    {
        #region variables

        private string _name;
        private Node _node;
        private double _voltageMagnitude;
        private double _realPower;

        #endregion

        #region constructor

        public Generator()
        {
            VoltageMagnitude = 1;
            RealPower = 0;
            Name = "";
        }

        public Generator(IReadOnlyDictionary<int, Node> nodeIds, ISafeDatabaseRecord reader)
        {
            var nodeId = reader.Parse<int>("Node");
            var node = nodeIds[nodeId];
            Id = reader.Parse<int>("GeneratorId");
            Name = reader.Parse<string>("GeneratorName");
            VoltageMagnitude = reader.Parse<double>("VoltageMagnitude");
            RealPower = reader.Parse<double>("RealPower");
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

        public double VoltageMagnitude
        {
            get { return _voltageMagnitude; }
            set
            {
                if (_voltageMagnitude == value) return;

                _voltageMagnitude = value;
                NotifyPropertyChanged();
            }
        }

        public double RealPower
        {
            get { return _realPower; }
            set
            {
                if (_realPower == value) return;

                _realPower = value;
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
                    "INSERT INTO generators (GeneratorName, PowerNet, VoltageMagnitude, RealPower) OUTPUT INSERTED.GeneratorId VALUES(@Name, @PowerNet, @VoltageMagnitude, @RealPower);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("VoltageMagnitude", SqlDbType.Real) { Value = VoltageMagnitude });
            command.Parameters.Add(new SqlParameter("RealPower", SqlDbType.Real) { Value = RealPower });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE generators SET Node=@Node, GeneratorName=@Name, VoltageMagnitude=@VoltageMagnitude, RealPower=@RealPower WHERE GeneratorId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("Node", SqlDbType.Int) { Value = NodeForeignKey });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("VoltageMagnitude", SqlDbType.Real) { Value = VoltageMagnitude });
            command.Parameters.Add(new SqlParameter("RealPower", SqlDbType.Real) { Value = RealPower });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM generators WHERE GeneratorId=@Id;");
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
                "CREATE TABLE generators " +
                "(GeneratorId INTEGER NOT NULL IDENTITY, Node INTEGER REFERENCES nodes (NodeId), PowerNet INTEGER NOT NULL REFERENCES powernets (PowerNetId), " +
                "GeneratorName TEXT NOT NULL, VoltageMagnitude REAL NOT NULL, RealPower REAL NOT NULL, " +
                "PRIMARY KEY(GeneratorId));");
        }

        public static SqlCommand CreateCommandToFetchAll(int powerNetId)
        {
            var command =
                new SqlCommand(
                    "SELECT * " +
                    "FROM generators WHERE PowerNet=@PowerNet;");
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            return command;
        }

        #endregion
    }
}