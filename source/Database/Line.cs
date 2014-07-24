using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Line : INotifyPropertyChanged
    {
        #region variables

        private string _name;
        private Node _nodeOne;
        private Node _nodeTwo;
        private double _seriesResistancePerUnitLength;
        private double _seriesInductancePerUnitLength;
        private double _shuntConductancePerUnitLength;
        private double _shuntCapacityPerUnitLength;
        private double _length;

        #endregion

        #region constructor

        public Line()
        {
            SeriesResistancePerUnitLength = 1;
            SeriesInductancePerUnitLength = 1;
            Length = 1;
            ShuntCapacityPerUnitLength = 0;
            ShuntConductancePerUnitLength = 0;
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

        public Node NodeOne
        {
            get { return _nodeOne; }
            set
            {
                if (_nodeOne == value) return;

                _nodeOne = value;
                NotifyPropertyChanged();
            }
        }

        public Node NodeTwo
        {
            get { return _nodeTwo; }
            set
            {
                if (_nodeTwo == value) return;

                _nodeTwo = value;
                NotifyPropertyChanged();
            }
        }

        public double SeriesResistancePerUnitLength
        {
            get { return _seriesResistancePerUnitLength; }
            set
            {
                if (_seriesResistancePerUnitLength == value) return;

                _seriesResistancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double SeriesInductancePerUnitLength
        {
            get { return _seriesInductancePerUnitLength; }
            set
            {
                if (_seriesInductancePerUnitLength == value) return;

                _seriesInductancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double ShuntConductancePerUnitLength
        {
            get { return _shuntConductancePerUnitLength; }
            set
            {
                if (_shuntConductancePerUnitLength == value) return;

                _shuntConductancePerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double ShuntCapacityPerUnitLength
        {
            get { return _shuntCapacityPerUnitLength; }
            set
            {
                if (_shuntCapacityPerUnitLength == value) return;

                _shuntCapacityPerUnitLength = value;
                NotifyPropertyChanged();
            }
        }

        public double Length
        {
            get { return _length; }
            set
            {
                if (_length == value) return;

                _length = value;
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
