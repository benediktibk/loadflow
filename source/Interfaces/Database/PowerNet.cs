using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Misc;

namespace Database
{
    public class PowerNet : IReadOnlyPowerNet
    {
        private ObservableCollection<Node> _nodes; 
        private ObservableCollection<TransmissionLine> _transmissionLines;
        private ObservableCollection<Load> _loads;
        private ObservableCollection<FeedIn> _feedIns;
        private ObservableCollection<Generator> _generators;
        private ObservableCollection<Transformer> _transformers;
        private double _frequency;
        private string _name;
        private long _netElementCount;
        private readonly List<string> _nodeNames;
        private string _logMessages;
        private Selection _calculatorSelection;

        public PowerNet()
        {
            Frequency = 50;
            Name = "";
            ReactToChangesWithDatabaseUpdate = true;
            _nodes = new ObservableCollection<Node>();
            _transmissionLines = new ObservableCollection<TransmissionLine>();
            _loads = new ObservableCollection<Load>();
            _feedIns = new ObservableCollection<FeedIn>();
            _generators = new ObservableCollection<Generator>();
            _transformers = new ObservableCollection<Transformer>();
            _transmissionLines.CollectionChanged += UpdateNetElementCount;
            _transmissionLines.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _loads.CollectionChanged += UpdateNetElementCount;
            _loads.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _feedIns.CollectionChanged += UpdateNetElementCount;
            _feedIns.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _generators.CollectionChanged += UpdateNetElementCount;
            _generators.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _transformers.CollectionChanged += UpdateNetElementCount;
            _transformers.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _nodes.CollectionChanged += UpdateNodeAbonnements;
            _nodes.CollectionChanged += UpdateDatabaseWithChangedNetElements;
            _netElementCount = 0;
            _nodeNames = new List<string>();
            _calculatorSelection = Selection.CurrentIteration;
        }

        public PowerNet(ISafeDatabaseRecord reader, IConnectionNetElements connection) : 
            this()
        {
            Id = reader.Parse<int>("PowerNetId");
            Name = reader.Parse<string>("PowerNetName");
            Frequency = reader.Parse<double>("Frequency");
            CalculatorSelection = (Selection)reader.Parse<int>("CalculatorSelection");
            Connection = connection;
        }

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

        public Selection CalculatorSelection
        {
            get { return _calculatorSelection; }
            set
            {
                if (_calculatorSelection == value) return;

                _calculatorSelection = value;
                NotifyPropertyChanged();
            }
        }

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

        public ObservableCollection<TransmissionLine> TransmissionLines
        {
            get { return _transmissionLines; }
            set
            {
                if (_transmissionLines == value) return;

                _transmissionLines = value;
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

        public IReadOnlyList<string> NodeNames
        {
            get { return _nodeNames; }
        }

        public string LogMessages
        {
            get { return _logMessages; }
            private set
            {
                if (_logMessages == value) return;

                _logMessages = value;
                NotifyPropertyChanged();
            }
        }

        public bool ReactToChangesWithDatabaseUpdate { get; set; }

        public IConnectionNetElements Connection { get; set; }
        
        public delegate void NodesChangedEventHandler();

        public event NodesChangedEventHandler NodesChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsNodeInUse(Node node)
        {
            var inUse = FeedIns.Aggregate(false, (current, element) => current || element.UsesNode(node));
            inUse = Generators.Aggregate(inUse, (current, element) => current || element.UsesNode(node));
            inUse = Loads.Aggregate(inUse, (current, element) => current || element.UsesNode(node));
            inUse = TransmissionLines.Aggregate(inUse, (current, element) => current || element.UsesNode(node));
            inUse = Transformers.Aggregate(inUse, (current, element) => current || element.UsesNode(node));
            return inUse;
        }

        public void Log(string message)
        {
            LogMessages += message + "\r\n";
        }

        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            NotifyPropertyChangedInternal(propertyName);
        }

        protected void NotifyPropertyChangedInternal(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateNetElementCount(object sender, NotifyCollectionChangedEventArgs e)
        {
            NetElementCount = _transmissionLines.Count + _loads.Count + _feedIns.Count + _generators.Count + _transformers.Count;
        }

        private void UpdateNodeAbonnements(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateNodeNames();

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newNodes = e.NewItems.Cast<Node>();
                foreach (var node in newNodes)
                {
                    node.NameChanged += UpdateNodeNames;
                    node.NameChanged += TriggerNodesChanged;
                }
            }

            if (NodesChanged != null)
                NodesChanged();
        }

        private void UpdateNodeNames()
        {
            _nodeNames.Clear();

            foreach (var node in _nodes)
                _nodeNames.Add(NodeToNodeNameConverter.Convert(node));

            NotifyPropertyChangedInternal("NodeNames");
        }

        private void TriggerNodesChanged()
        {
            if (NodesChanged != null)
                NodesChanged();
        }

        private void UpdateDatabaseWithChangedNetElements(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReactToChangesWithDatabaseUpdate)
            {
                Connection.AddList(e.NewItems, Id);
                Connection.RemoveList(e.OldItems);
            }

            if (e.NewItems == null)
                return;

            var elements = e.NewItems.Cast<INetElement>();
            foreach (var element in elements)
                element.PropertyChanged += UpdateDatabaseWithChangedNetElement;
        }

        private void UpdateDatabaseWithChangedNetElement(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as INetElement;

            if (node == null)
                throw new ArgumentNullException("sender");

            Connection.Update(node);
        }
    }
}
