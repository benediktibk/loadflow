using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Node : INetElement
    {
        #region variables

        private string _name;
        private double _nominalVoltage;
        private double _voltageReal;
        private double _voltageImaginary;

        #endregion

        #region constructor

        public Node()
        {
            NominalVoltage = 1;
            VoltageReal = 0;
            VoltageImaginary = 0;
            Name = "";
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
                if (NameChanged != null)
                    NameChanged();
            }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
            set
            {
                if (_nominalVoltage == value) return;

                _nominalVoltage = value;
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

        #endregion

        #region events

        public delegate void NameChangedEventHandler();

        public event NameChangedEventHandler NameChanged;

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
                    "INSERT INTO nodes (NodeName, PowerNet, NominalVoltage, NodeVoltageReal, NodeVoltageImaginary) OUTPUT INSERTED.NodeId VALUES(@Name, @PowerNet, @NominalVoltage, @VoltageReal, @VoltageImaginary);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("PowerNet", SqlDbType.Int) { Value = powerNetId });
            command.Parameters.Add(new SqlParameter("NominalVoltage", SqlDbType.Real) { Value = NominalVoltage });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) { Value = VoltageImaginary });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE nodes SET NodeName=@Name, NominalVoltage=@NominalVoltage, NodeVoltageReal=@VoltageReal, NodeVoltageImaginary=@VoltageImaginary WHERE NodeId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("NominalVoltage", SqlDbType.Real) { Value = NominalVoltage });
            command.Parameters.Add(new SqlParameter("VoltageReal", SqlDbType.Real) { Value = VoltageReal });
            command.Parameters.Add(new SqlParameter("VoltageImaginary", SqlDbType.Real) { Value = VoltageImaginary });
            return command;
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            var command = new SqlCommand("DELETE FROM nodes WHERE NodeId=@Id;");
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            return command;
        }

        public bool UsesNode(Node node)
        {
            return node == this;
        }

        #endregion
    }
}
