using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using Calculation;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;
using Misc;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;

namespace Database
{
    public class PowerNetComputable : PowerNet
    {
        private bool _isCalculationRunning;
        private Mutex _isCalculationRunningMutex;
        private BackgroundWorker _backgroundWorker;
        private SymmetricPowerNet _symmetricPowerNet;
        private IReadOnlyDictionary<int, NodeResult> _nodeResults;
        private Factory _calculatorFactory;

        public PowerNetComputable()
        {
            Initialize();
        }

        public PowerNetComputable(ISafeDatabaseRecord reader, IConnectionNetElements connection) : base(reader, connection)
        {
            Initialize();
        }

        public bool IsCalculationNotRunning
        {
            get { return !IsCalculationRunning; }
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
            var nodeVoltageCalculator = _calculatorFactory.CreateNodeVoltageCalculator(CalculatorSelection);

            if (!CreatePowerNet(nodeVoltageCalculator))
                return;

            Log("starting with calculation of node voltages");
            _backgroundWorker.RunWorkerAsync();
        }

        public bool CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerBase)
        {
            matrix = null;
            nodeNames = null;
            powerBase = 0;

            if (!CreatePowerNet(null))
                return false;

            _symmetricPowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);

            return true;
        }

        private void Initialize()
        {
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += CalculateNodeVoltages;
            _backgroundWorker.RunWorkerCompleted += CalculationFinished;
            _calculatorFactory = new Factory
            {
                TargetPrecision = 0.000001,
                MaximumIterations = 1000,
                BitPrecision = 200,
                CoefficientCount = 50
            };
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
                double relativePowerError;
                _nodeResults = _symmetricPowerNet.CalculateNodeVoltages(out relativePowerError);

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

            var singleVoltagePowerNetFactory = new PowerNetFactory(nodeVoltageCalculator);
            var singlePhasePowerNet = new Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable(Frequency, singleVoltagePowerNetFactory, new NodeGraph());
            _symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);

            try
            {
                foreach (var node in Nodes)
                    _symmetricPowerNet.AddNode(node.Id, node.NominalVoltage, node.Name);

                foreach (var line in TransmissionLines)
                    _symmetricPowerNet.AddTransmissionLine(line.NodeOne.Id, line.NodeTwo.Id, line.SeriesResistancePerUnitLength,
                        line.SeriesInductancePerUnitLength, line.ShuntConductancePerUnitLength,
                        line.ShuntCapacityPerUnitLength, line.Length, line.TransmissionEquationModel);

                foreach (var feedIn in FeedIns)
                {
                    var nominalVoltage = feedIn.Node.NominalVoltage;
                    var Z = feedIn.C * nominalVoltage * nominalVoltage / feedIn.ShortCircuitPower;
                    var X = Math.Sqrt(feedIn.RealToImaginary * feedIn.RealToImaginary + 1) / Z;
                    var R = feedIn.RealToImaginary * X;
                    var internalImpedance = new Complex(R, X);
                    _symmetricPowerNet.AddFeedIn(feedIn.Node.Id,
                        new Complex(feedIn.VoltageReal, feedIn.VoltageImaginary),
                        internalImpedance);
                }

                foreach (var generator in Generators)
                    _symmetricPowerNet.AddGenerator(generator.Node.Id, generator.VoltageMagnitude, generator.RealPower);

                foreach (var load in Loads)
                    _symmetricPowerNet.AddLoad(load.Node.Id, new Complex(load.Real, load.Imaginary));

                foreach (var transformer in Transformers)
                    _symmetricPowerNet.AddTwoWindingTransformer(transformer.UpperSideNode.Id, transformer.LowerSideNode.Id,
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
    }
}
