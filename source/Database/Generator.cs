using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Generator: INotifyPropertyChanged
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
    }
}
