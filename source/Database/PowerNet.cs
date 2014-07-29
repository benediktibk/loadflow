using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

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
        private bool _isCalculationRunning;
        private readonly Mutex _isCalculationRunningMutex;
        private readonly BackgroundWorker _backgroundWorker;
        private SymmetricPowerNet _calculationPowerNet;
        private INodeVoltageCalculator _nodeVoltageCalculator;
        private string _logMessages;
        private NodeVoltageCalculatorSelection _calculatorSelection;

        #endregion

        #region constructor

        public PowerNet()
        {
            Frequency = 50;
            Name = "";
            ReactToChangesWithDatabaseUpdate = true;
            _nodes = new ObservableCollection<Node>();
            _lines = new ObservableCollection<Line>();
            _loads = new ObservableCollection<Load>();
            _feedIns = new ObservableCollection<FeedIn>();
            _generators = new ObservableCollection<Generator>();
            _transformers = new ObservableCollection<Transformer>();
            _lines.CollectionChanged += UpdateNetElementCount;
            _lines.CollectionChanged += UpdateDatabaseWithChangedLines;
            _loads.CollectionChanged += UpdateNetElementCount;
            _loads.CollectionChanged += UpdateDataBaseWithChangedLoads;
            _feedIns.CollectionChanged += UpdateNetElementCount;
            _feedIns.CollectionChanged += UpdateDatabaseWithChangedFeedIns;
            _generators.CollectionChanged += UpdateNetElementCount;
            _generators.CollectionChanged += UpdateDatabaseWithChangedGenerators;
            _transformers.CollectionChanged += UpdateNetElementCount;
            _transformers.CollectionChanged += UpdateDatabaseWithChangedTransformers;
            _nodes.CollectionChanged += UpdateNodeAbonnements;
            _nodes.CollectionChanged += UpdateDatabaseWithChangedNodes;
            _netElementCount = 0;
            _nodeNames = new List<string>();
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += CalculateNodeVoltages;
            _backgroundWorker.RunWorkerCompleted += CalculationFinished;
            _calculatorSelection = NodeVoltageCalculatorSelection.CurrentIteration;
        }

        #endregion

        #region public functions

        public void CalculateNodeVoltagesInBackground()
        {
            _isCalculationRunningMutex.WaitOne();
            if (_isCalculationRunning)
            {
                _isCalculationRunningMutex.ReleaseMutex();
                Log("calculation already running");
                return;
            }
            _isCalculationRunning = true;
            _isCalculationRunningMutex.ReleaseMutex();

            Log("creating symmetric power net");

            _calculationPowerNet = new SymmetricPowerNet(Frequency);
            _nodeVoltageCalculator = NodeVoltageCalculatorFactory.Create(CalculatorSelection);

            try
            {
                foreach (var node in Nodes)
                    _calculationPowerNet.AddNode(node.Id, node.NominalVoltage);

                foreach (var line in Lines)
                    _calculationPowerNet.AddLine(line.NodeOne.Id, line.NodeTwo.Id, line.SeriesResistancePerUnitLength,
                        line.SeriesInductancePerUnitLength, line.ShuntConductancePerUnitLength,
                        line.ShuntCapacityPerUnitLength, line.Length);

                foreach (var feedIn in FeedIns)
                    _calculationPowerNet.AddFeedIn(feedIn.Node.Id,
                        new Complex(feedIn.VoltageReal, feedIn.VoltageImaginary),
                        feedIn.ShortCircuitPower);

                foreach (var generator in Generators)
                    _calculationPowerNet.AddGenerator(generator.Node.Id, generator.VoltageMagnitude, generator.RealPower);

                foreach (var load in Loads)
                    _calculationPowerNet.AddLoad(load.Node.Id, new Complex(load.Real, load.Imaginary));

                foreach (var transformer in Transformers)
                    _calculationPowerNet.AddTransformer(transformer.UpperSideNode.Id, transformer.LowerSideNode.Id,
                        transformer.NominalPower, transformer.RelativeShortCircuitVoltage, transformer.CopperLosses,
                        transformer.IronLosses, transformer.RelativeNoLoadCurrent, transformer.Ratio);
            } 
            catch (Exception exception)
            {
                Log("an error occured during the creation of the net: " + exception.Message);
                IsCalculationRunning = false;
                return;
            }

            Log("starting with calculation of node voltages");
            _backgroundWorker.RunWorkerAsync();
        }

        #endregion

        #region properties

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

        public NodeVoltageCalculatorSelection CalculatorSelection
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

        public IReadOnlyList<string> NodeNames
        {
            get { return _nodeNames; }
        }

        public bool IsCalculationNotRunning
        {
            get { return !IsCalculationRunning; }
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

        public bool IsCalculationRunning
        {
            get
            {
                _isCalculationRunningMutex.WaitOne();
                var isCalculationRunning = _isCalculationRunning;
                _isCalculationRunningMutex.ReleaseMutex();
                return isCalculationRunning;
            }
            set
            {
                var changed = false;

                _isCalculationRunningMutex.WaitOne();
                if (_isCalculationRunning != value)
                {
                    _isCalculationRunning = value;
                    changed = true;
                }
                _isCalculationRunningMutex.ReleaseMutex();

                if (!changed) return;
                NotifyPropertyChanged();
                NotifyPropertyChangedInternal("IsCalculationNotRunning");
            }
        }

        public bool ReactToChangesWithDatabaseUpdate { get; set; }

        public IConnectionNetElements Connection { get; set; }

        #endregion

        #region events

        public delegate void NodesChangedEventHandler();

        public event NodesChangedEventHandler NodesChanged;

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

        private void UpdateNetElementCount(object sender, NotifyCollectionChangedEventArgs e)
        {
            NetElementCount = _lines.Count + _loads.Count + _feedIns.Count + _generators.Count + _transformers.Count;
        }

        void UpdateNodeAbonnements(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateNodeNames();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newNodes = e.NewItems.Cast<Node>();
                    foreach (var node in newNodes)
                    {
                        node.NameChanged += UpdateNodeNames;
                        node.NameChanged += TriggerNodesChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (NodesChanged != null)
                NodesChanged();
        }

        void UpdateNodeNames()
        {
            _nodeNames.Clear();

            foreach (var node in _nodes)
                _nodeNames.Add(NodeToNodeNameConverter.Convert(node));

            NotifyPropertyChangedInternal("NodeNames");
        }

        void TriggerNodesChanged()
        {
            if (NodesChanged != null)
                NodesChanged();
        }

        private void CalculationFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_calculationPowerNet != null)
            {
                foreach (var node in Nodes)
                {
                    var voltage = _calculationPowerNet.GetNodeVoltage(node.Id);
                    node.VoltageReal = voltage.Real;
                    node.VoltageImaginary = voltage.Imaginary;
                }
            }

            Log("finished calculation");
            IsCalculationRunning = false;
        }

        private void CalculateNodeVoltages(object sender, DoWorkEventArgs e)
        {
            try
            {
                _calculationPowerNet.CalculateNodeVoltages(_nodeVoltageCalculator);
            }
            catch (Exception exception)
            {
                Log("an error occurred: " + exception.Message);
                _calculationPowerNet = null;
            }
        }

        private void Log(string message)
        {
            LogMessages += message + "\r\n";
        }

        #endregion

        #region database update

        private void UpdateDatabaseWithChangedNodes(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newNodes = e.NewItems.Cast<Node>();
                    foreach (var node in newNodes)
                    {
                        Connection.Add(node, this);
                        node.PropertyChanged += UpdateDatabaseWithChangedNode;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldNodes = e.OldItems.Cast<Node>();
                    foreach (var node in oldNodes)
                        Connection.Remove(node);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedNode(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as Node;

            if (node == null)
                throw new ArgumentNullException("sender");

            Connection.Update(node);
        }

        private void UpdateDatabaseWithChangedLines(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newLines = e.NewItems.Cast<Line>();
                    foreach (var line in newLines)
                    {
                        Connection.Add(line, this);
                        line.PropertyChanged += UpdateDatabaseWithChangedLine;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldLines = e.OldItems.Cast<Line>();
                    foreach (var line in oldLines)
                        Connection.Remove(line);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedLine(object sender, PropertyChangedEventArgs e)
        {
            var line = sender as Line;

            if (line == null)
                throw new ArgumentNullException("sender");

            Connection.Update(line);
        }

        private void UpdateDataBaseWithChangedLoads(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newLoads = e.NewItems.Cast<Load>();
                    foreach (var load in newLoads)
                    {
                        Connection.Add(load, this);
                        load.PropertyChanged += UpdateDatabaseWithChangedLoad;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldLoads = e.OldItems.Cast<Load>();
                    foreach (var load in oldLoads)
                        Connection.Remove(load);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedLoad(object sender, PropertyChangedEventArgs e)
        {
            var load = sender as Load;

            if (load == null)
                throw new ArgumentNullException("sender");

            Connection.Update(load);
        }

        private void UpdateDatabaseWithChangedFeedIns(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newFeedIns = e.NewItems.Cast<FeedIn>();
                    foreach (var feedIn in newFeedIns)
                    {
                        Connection.Add(feedIn, this);
                        feedIn.PropertyChanged += UpdateDatabaseWithChangedFeedIn;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldFeedIns = e.OldItems.Cast<FeedIn>();
                    foreach (var feedIn in oldFeedIns)
                        Connection.Remove(feedIn);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedFeedIn(object sender, PropertyChangedEventArgs e)
        {
            var feedIn = sender as FeedIn;

            if (feedIn == null)
                throw new ArgumentNullException("sender");

            Connection.Update(feedIn);
        }

        private void UpdateDatabaseWithChangedGenerators(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newGenerators = e.NewItems.Cast<Generator>();
                    foreach (var generator in newGenerators)
                    {
                        Connection.Add(generator, this);
                        generator.PropertyChanged += UpdateDatabaseWithChangedGenerator;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldGenerators = e.OldItems.Cast<Generator>();
                    foreach (var generator in oldGenerators)
                        Connection.Remove(generator);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedGenerator(object sender, PropertyChangedEventArgs e)
        {
            var generator = sender as Generator;

            if (generator == null)
                throw new ArgumentNullException("sender");

            Connection.Update(generator);
        }

        private void UpdateDatabaseWithChangedTransformers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!ReactToChangesWithDatabaseUpdate)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newTransformers = e.NewItems.Cast<Transformer>();
                    foreach (var transformer in newTransformers)
                    {
                        Connection.Add(transformer, this);
                        transformer.PropertyChanged += UpdateDatabaseWithChangedTransformer;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldTransformers = e.OldItems.Cast<Transformer>();
                    foreach (var transformer in oldTransformers)
                        Connection.Remove(transformer);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateDatabaseWithChangedTransformer(object sender, PropertyChangedEventArgs e)
        {
            var transformer = sender as Transformer;

            if (transformer == null)
                throw new ArgumentNullException("sender");

            Connection.Update(transformer);
        }

        #endregion
    }
}
