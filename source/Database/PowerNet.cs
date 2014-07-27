using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Database
{
    public class PowerNet : INotifyPropertyChanged
    {
        #region variables

        private ObservableCollection<Node> _nodes; 
        private ObservableCollection<Line> _lines;
        private ObservableCollection<Load> _loads;
        private ObservableCollection<FeedIn> _feedIns;
        private ObservableCollection<Generator> _generators;
        private ObservableCollection<Transformer> _transformers;
        private double _frequency;
        private string _name;
        private long _netElementCount;
        private readonly List<string> _nodeNames; 

        #endregion

        #region constructor

        public PowerNet()
        {
            Frequency = 50;
            _nodes = new ObservableCollection<Node>();
            _lines = new ObservableCollection<Line>();
            _loads = new ObservableCollection<Load>();
            _feedIns = new ObservableCollection<FeedIn>();
            _generators = new ObservableCollection<Generator>();
            _transformers = new ObservableCollection<Transformer>();
            _lines.CollectionChanged += NetElementCountChanged;
            _loads.CollectionChanged += NetElementCountChanged;
            _feedIns.CollectionChanged += NetElementCountChanged;
            _generators.CollectionChanged += NetElementCountChanged;
            _transformers.CollectionChanged += NetElementCountChanged;
            _netElementCount = 0;
            _nodeNames = new List<string>();
            _nodes.CollectionChanged += NodeCollectionChanged;
        }

        #endregion

        #region properties

        [Key]
        public int Id { get; set; }

        public double Frequency
        {
            get { return _frequency; }
            set
            {
                if (_frequency == value) return;

                _frequency = value;
                NotifyPropertyChanged();
            }
        }

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

        [NotMapped]
        public long NetElementCount
        {
            get { return _netElementCount; }
            private set
            {
                if (_netElementCount == value) return;

                _netElementCount = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Node> Nodes
        {
            get { return _nodes; }
            set
            {
                if (_nodes == value) return;

                _nodes = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Line> Lines
        {
            get { return _lines; }
            set
            {
                if (_lines == value) return;

                _lines = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Load> Loads
        {
            get { return _loads; }
            set
            {
                if (_loads == value) return;

                _loads = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<FeedIn> FeedIns
        {
            get { return _feedIns; }
            set
            {
                if (_feedIns == value) return;

                _feedIns = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Generator> Generators
        {
            get { return _generators; }
            set
            {
                if (_generators == value) return;

                _generators = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Transformer> Transformers
        {
            get { return _transformers; }
            set
            {
                if (_transformers == value) return;

                _transformers = value;
                NotifyPropertyChanged();
            }
        }

        [NotMapped]
        public IReadOnlyList<string> NodeNames
        {
            get { return _nodeNames; }
        }

        #endregion

        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            NotifyPropertyChangedInternal(propertyName);
        }

        private void NotifyPropertyChangedInternal(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region private functions

        private void NetElementCountChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NetElementCount = _lines.Count + _loads.Count + _feedIns.Count + _generators.Count + _transformers.Count;
        }

        void NodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateNodeNames();

            foreach (var node in _nodes)
                node.PropertyChanged += NodePropertyChanged;
        }

        void NodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Name" && e.PropertyName != "Id")
                return;

            UpdateNodeNames();
        }

        void UpdateNodeNames()
        {
            _nodeNames.Clear();

            foreach (var node in _nodes)
                _nodeNames.Add(NodeToNodeNameConverter.Convert(node));

            NotifyPropertyChangedInternal("NodeNames");
        }

        #endregion
    }
}
