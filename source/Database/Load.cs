using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Load : INotifyPropertyChanged, INetElement
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
            throw new NotImplementedException();
        }

        public SqlCommand CreateCommandToRemoveFromDatabase()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
