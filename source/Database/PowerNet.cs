using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Database.Annotations;

namespace Database
{
    public class PowerNet : INotifyPropertyChanged
    {
        #region variables

        private Nodes _nodes; 
        private Lines _lines;
        private Loads _loads;
        private FeedIns _feedIns;
        private Generators _generators;
        private Transformers _transformers;

        #endregion

        #region constructor

        public PowerNet()
        {
            Frequency = 50;
            _nodes = new Nodes();
            _lines = new Lines();
            _loads = new Loads();
            _feedIns = new FeedIns();
            _generators = new Generators();
            _transformers = new Transformers();
        }

        #endregion

        #region properties

        public double Frequency { get; set; }
        public string Name { get; set; }

        public int NetElementCount
        {
            get { return Lines.Count + Loads.Count + FeedIns.Count + Generators.Count; }
        }

        public Nodes Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes == value) return;

                _nodes = value;
                NotifyPropertyChanged();
            }
        }

        public Lines Lines
        {
            get { return _lines; }
            set
            {
                if (_lines == value) return;

                _lines = value;
                NotifyPropertyChanged();
            }
        }

        public Loads Loads
        {
            get { return _loads; }
            set
            {
                if (_loads == value) return;

                _loads = value;
                NotifyPropertyChanged();
            }
        }

        public FeedIns FeedIns
        {
            get { return _feedIns; }
            set
            {
                if (_feedIns == value) return;

                _feedIns = value;
                NotifyPropertyChanged();
            }
        }

        public Generators Generators
        {
            get { return _generators; }
            set
            {
                if (_generators == value) return;

                _generators = value;
                NotifyPropertyChanged();
            }
        }

        public Transformers Transformers
        {
            get { return _transformers; }
            set
            {
                if (_transformers == value) return;

                _transformers = value;
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
