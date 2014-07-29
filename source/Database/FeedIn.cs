using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class FeedIn : INotifyPropertyChanged
    {
        #region variables

        private string _name;
        private Node _node;
        private double _voltageReal;
        private double _voltageImaginary;
        private double _shortCircuitPower;

        #endregion

        #region constructor

        public FeedIn()
        {
            VoltageReal = 1;
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
