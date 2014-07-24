using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Database
{
    public class FeedIn : INotifyPropertyChanged
    {
        #region variables

        private string _name;
        private Node _node;
        private Complex _voltage;
        private double _shortCircuitPower;

        #endregion

        #region constructor

        public FeedIn()
        {
            Voltage = new Complex(1, 0);
        }

        #endregion

        #region properties

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

        public Complex Voltage
        {
            get { return _voltage; }
            set
            {
                if (_voltage == value) return;

                _voltage = value;
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
