using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Calculation;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;

namespace Database
{
    public class PowerNet : INotifyPropertyChanged
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
        private bool _isCalculationRunning;
        private readonly Mutex _isCalculationRunningMutex;
        private readonly BackgroundWorker _backgroundWorker;
        private SymmetricPowerNet _calculationPowerNet;
        private IReadOnlyDictionary<long, NodeResult> _nodeResults;
        private string _logMessages;
        private NodeVoltageCalculatorSelection _calculatorSelection;

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
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += CalculateNodeVoltages;
            _backgroundWorker.RunWorkerCompleted += CalculationFinished;
            _calculatorSelection = NodeVoltageCalculatorSelection.CurrentIteration;
        }

        public PowerNet(ISafeDatabaseRecord reader, IConnectionNetElements connection) : 
            this()
        {
            Id = reader.Parse<int>("PowerNetId");
            Name = reader.Parse<string>("PowerNetName");
            Frequency = reader.Parse<double>("Frequency");
            CalculatorSelection = (NodeVoltageCalculatorSelection) reader.Parse<int>("CalculatorSelection");
            Connection = connection;
        }

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

            const double targetPrecision = 0.000001;
            const int maximumIterations = 1000;
            const int helmBitPrecisionMulti = 200;
            const int coefficientCountHelmMulti = 80;
            const int coefficientCountHelmLongDouble = 50;
            INodeVoltageCalculator nodeVoltageCalculator;

            switch (CalculatorSelection)
            {
                case NodeVoltageCalculatorSelection.NodePotential:
                    nodeVoltageCalculator = new NodePotentialMethod();
                    break;
                case NodeVoltageCalculatorSelection.CurrentIteration:
                    nodeVoltageCalculator = new CurrentIteration(targetPrecision, maximumIterations);
                    break;
                case NodeVoltageCalculatorSelection.NewtonRaphson:
                    nodeVoltageCalculator = new NewtonRaphsonMethod(targetPrecision, maximumIterations);
                    break;
                case NodeVoltageCalculatorSelection.FastDecoupledLoadFlow:
                    nodeVoltageCalculator = new FastDecoupledLoadFlowMethod(targetPrecision, maximumIterations);
                    break;
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlow:
                    nodeVoltageCalculator =
                        new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, coefficientCountHelmLongDouble,
                            new PrecisionLongDouble());
                    break;
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowHighPrecision:
                    nodeVoltageCalculator =
                        new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, coefficientCountHelmMulti,
                            new PrecisionMulti(helmBitPrecisionMulti));
                    break;
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowWithCurrentIteration:
                    nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision, new CurrentIteration(targetPrecision, maximumIterations));
                    break;
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowWithNewtonRaphson:
                    nodeVoltageCalculator = new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision, new NewtonRaphsonMethod(targetPrecision, maximumIterations));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!CreatePowerNet(nodeVoltageCalculator)) 
                return;

            Log("starting with calculation of node voltages");
            _backgroundWorker.RunWorkerAsync();
        }

        public bool CalculateAdmittanceMatrix(out Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerBase)
        {
            matrix = null;
            nodeNames = null;
            powerBase = 0;

            if (!CreatePowerNet(null))
                return false;

            _calculationPowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);

            return true;
        }

        public SqlCommand CreateCommandToAddToDatabase()
        {
            var command =
                new SqlCommand(
                    "INSERT INTO powernets (PowerNetName, Frequency, CalculatorSelection) OUTPUT INSERTED.PowerNetId VALUES(@Name, @Frequency, @CalculatorSelection);");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("Frequency", SqlDbType.Real) { Value = Frequency });
            command.Parameters.Add(new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = CalculatorSelection });
            return command;
        }

        public SqlCommand CreateCommandToUpdateInDatabase()
        {
            var command =
                new SqlCommand(
                    "UPDATE powernets SET PowerNetName=@Name, Frequency=@Frequency, CalculatorSelection=@CalculatorSelection WHERE PowerNetId=@Id;");
            command.Parameters.Add(new SqlParameter("Name", SqlDbType.Text) { Value = Name });
            command.Parameters.Add(new SqlParameter("Frequency", SqlDbType.Real) { Value = Frequency });
            command.Parameters.Add(new SqlParameter("CalculatorSelection", SqlDbType.Int) { Value = CalculatorSelection });
            command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });
            return command;
        }

        public IEnumerable<SqlCommand> CreateCommandsToRemoveFromDatabase()
        {
            var deletePowerNetCommand = new SqlCommand("DELETE FROM powernets WHERE PowerNetId=@Id;");
            var deleteNodesCommand = new SqlCommand("DELETE FROM nodes WHERE PowerNet=@Id;");
            var deleteLoadsCommand = new SqlCommand("DELETE FROM loads WHERE PowerNet=@Id;");
            var deleteFeedInsCommand = new SqlCommand("DELETE FROM feedins WHERE PowerNet=@Id;");
            var deleteGeneratorsCommand = new SqlCommand("DELETE FROM generators WHERE PowerNet=@Id;");
            var deleteTransformersCommand = new SqlCommand("DELETE FROM transformers WHERE PowerNet=@Id;");
            var deleteLinesCommand = new SqlCommand("DELETE FROM lines WHERE PowerNet=@Id;");
            var commands = new List<SqlCommand>
            {
                deleteFeedInsCommand,
                deleteGeneratorsCommand,
                deleteTransformersCommand,
                deleteLinesCommand,
                deleteLoadsCommand,
                deleteNodesCommand,
                deletePowerNetCommand
            };

            foreach (var command in commands)
                command.Parameters.Add(new SqlParameter("Id", SqlDbType.Int) { Value = Id });

            return commands;
        }

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
        
        public delegate void NodesChangedEventHandler();

        public event NodesChangedEventHandler NodesChanged;

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

        private void CalculationFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_nodeResults != null)
            {
                foreach (var node in Nodes)
                {
                    var voltage = _nodeResults[node.Id].Voltage;
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
                _nodeResults = _calculationPowerNet.CalculateNodeVoltages();

                if (_nodeResults != null)
                    return;

                Log("voltage collapse");
            }
            catch (Exception exception)
            {
                Log("an error occurred: " + exception.Message);
            }
        }

        private bool CreatePowerNet(INodeVoltageCalculator nodeVoltageCalculator)
        {
            Log("creating symmetric power net");

            _calculationPowerNet = new SymmetricPowerNet(Frequency, nodeVoltageCalculator);

            try
            {
                foreach (var node in Nodes)
                    _calculationPowerNet.AddNode(node.Id, node.NominalVoltage, node.Name);

                foreach (var line in TransmissionLines)
                    _calculationPowerNet.AddTransmissionLine(line.NodeOne.Id, line.NodeTwo.Id, line.SeriesResistancePerUnitLength,
                        line.SeriesInductancePerUnitLength, line.ShuntConductancePerUnitLength,
                        line.ShuntCapacityPerUnitLength, line.Length, line.TransmissionEquationModel);

                foreach (var feedIn in FeedIns)
                    _calculationPowerNet.AddFeedIn(feedIn.Node.Id,
                        new Complex(feedIn.VoltageReal, feedIn.VoltageImaginary),
                        feedIn.ShortCircuitPower, feedIn.C, feedIn.RealToImaginary);

                foreach (var generator in Generators)
                    _calculationPowerNet.AddGenerator(generator.Node.Id, generator.VoltageMagnitude, generator.RealPower);

                foreach (var load in Loads)
                    _calculationPowerNet.AddLoad(load.Node.Id, new Complex(load.Real, load.Imaginary));

                foreach (var transformer in Transformers)
                    _calculationPowerNet.AddTwoWindingTransformer(transformer.UpperSideNode.Id, transformer.LowerSideNode.Id,
                        transformer.NominalPower, transformer.RelativeShortCircuitVoltage, transformer.CopperLosses,
                        transformer.IronLosses, transformer.RelativeNoLoadCurrent, transformer.Ratio, new Angle(), transformer.Name);
            }
            catch (Exception exception)
            {
                Log("an error occured during the creation of the net: " + exception.Message);
                IsCalculationRunning = false;
                return false;
            }

            return true;
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

        public static SqlCommand CreateCommandToCreateTable()
        {
            return new SqlCommand(
                "CREATE TABLE powernets " +
                "(PowerNetId INTEGER NOT NULL IDENTITY, Frequency REAL NOT NULL, PowerNetName TEXT NOT NULL, CalculatorSelection INTEGER NOT NULL, " +
                "PRIMARY KEY(PowerNetId));");
        }

        public static SqlCommand CreateCommandToFetchAll()
        {
            return new SqlCommand("SELECT * FROM powernets;");
        }
    }
}
