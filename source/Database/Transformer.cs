using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Transformer : INotifyPropertyChanged
    {
        #region variables

        private string _name;
        private Node _upperSideNode;
        private Node _lowerSideNode;
        private double _nominalPower;
        private double _relativeShortCircuitVoltage;
        private double _copperLosses;
        private double _ironLosses;
        private double _relativeNoLoadCurrent;
        private double _ratio;

        #endregion

        #region constructor

        public Transformer()
        {
            NominalPower = 10;
            RelativeShortCircuitVoltage = 0.01;
            CopperLosses = 0.01;
            IronLosses = 0.01;
            RelativeNoLoadCurrent = 0.01;
            Ratio = 1;
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

        public Node UpperSideNode
        {
            get { return _upperSideNode; }
            set
            {
                if (_upperSideNode == value) return;

                _upperSideNode = value;
                NotifyPropertyChanged();
            }
        }

        public Node LowerSideNode
        {
            get { return _lowerSideNode; }
            set
            {
                if (_lowerSideNode == value) return;

                _lowerSideNode = value;
                NotifyPropertyChanged();
            }
        }

        public double NominalPower
        {
            get { return _nominalPower; }
            set
            {
                if (_nominalPower == value) return;

                _nominalPower = value;
                NotifyPropertyChanged();
            }
        }

        public double RelativeShortCircuitVoltage
        {
            get { return _relativeShortCircuitVoltage; }
            set
            {
                if (_relativeShortCircuitVoltage == value) return;

                _relativeShortCircuitVoltage = value;
                NotifyPropertyChanged();
            }
        }

        public double CopperLosses
        {
            get { return _copperLosses; }
            set
            {
                if (_copperLosses == value) return;

                _copperLosses = value;
                NotifyPropertyChanged();
            }
        }

        public double IronLosses
        {
            get { return _ironLosses; }
            set
            {
                if (_ironLosses == value) return;

                _ironLosses = value;
                NotifyPropertyChanged();
            }
        }

        public double RelativeNoLoadCurrent
        {
            get { return _relativeNoLoadCurrent; }
            set
            {
                if (_relativeNoLoadCurrent == value) return;

                _relativeNoLoadCurrent = value;
                NotifyPropertyChanged();
            }
        }

        public double Ratio
        {
            get { return _ratio; }
            set
            {
                if (_ratio == value) return;

                _ratio = value;
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
