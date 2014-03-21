using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LoadFlowCalculationComparison.AlgorithmSettings;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.Statistics;

namespace LoadFlowCalculationComparison
{
    public partial class MainWindow
    {
        private readonly GeneralSettings _generalSettings;
        private readonly IterativeMethodSettings _currentIteration;
        private readonly IterativeMethodSettings _fastDecoupledLoadFlow;
        private readonly IterativeMethodSettings _newtonRaphson;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlow;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlowHighAccuracy;
        private readonly NodePotentialMethodSettings _nodePotential;
        private readonly CalculationResults _calculationResults;

        private delegate void ProblemOnceSolved();

        private delegate void ResultCalculated(CalculationResult result);

        public MainWindow()
        {
            _generalSettings = new GeneralSettings();
            _currentIteration = new IterativeMethodSettings(_generalSettings);
            _fastDecoupledLoadFlow = new IterativeMethodSettings(_generalSettings);
            _holomorphicEmbeddedLoadFlow = new HolomorphicEmbeddedLoadFlowMethodSettings(_generalSettings);
            _holomorphicEmbeddedLoadFlowHighAccuracy = new HolomorphicEmbeddedLoadFlowMethodSettings(_generalSettings);
            _newtonRaphson = new IterativeMethodSettings(_generalSettings);
            _nodePotential = new NodePotentialMethodSettings(_generalSettings);

            InitializeComponent();
            _calculationResults = FindResource("CalculationResults") as CalculationResults;
            NodePotentialGrid.DataContext = _nodePotential;
            HolomorphicEmbeddedLoadFlowGrid.DataContext = _holomorphicEmbeddedLoadFlow;
            HolomorphicEmbeddedLoadFlowHighAccuracyGrid.DataContext = _holomorphicEmbeddedLoadFlowHighAccuracy;
            FastDecoupledLoadFlowGrid.DataContext = _fastDecoupledLoadFlow;
            CurrentIterationGrid.DataContext = _currentIteration;
            NewtonRaphsonGrid.DataContext = _newtonRaphson;
            GeneralSettingsGrid.DataContext = _generalSettings;
            CalculateButton.DataContext = _generalSettings;
        }

        private void IncreaseProgressBarCount()
        {
            CalculateProgressBar.Value += 1;
        }

        private void AddCalculationResult(CalculationResult result)
        {
            _calculationResults.Add(result);
        }

        private void ProblemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nodePotentialSingularityDetection = 0.0;
            var currentIterationTargetPrecision = 0.0;
            var currentIterationMaximumIterations = 0;
            var newtonRaphsonTargetPrecision = 0.0;
            var newtonRaphsonMaximumIterations = 0;
            var fdlfTargetPrecision = 0.0;
            var fdlfMaximumIterations = 0;
            var helmTargetPrecision = 0.0;
            var helmMaximumNumberOfCoefficients = 0;

            switch (_generalSettings.ProblemSelection)
            {
                case ProblemSelectionEnum.CollapsingTwoNodeSystem:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmTargetPrecision = 0.00001;
                    helmMaximumNumberOfCoefficients = 50;
                    break;
                case ProblemSelectionEnum.StableTwoNodeSystem:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 10000;
                    helmTargetPrecision = 0.00001;
                    helmMaximumNumberOfCoefficients = 50;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithFourPQBuses:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 10000;
                    helmTargetPrecision = 0.001;
                    helmMaximumNumberOfCoefficients = 60;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithOneGroundNode:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 10000;
                    helmTargetPrecision = 0.00001;
                    helmMaximumNumberOfCoefficients = 46;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmTargetPrecision = 0.00001;
                    helmMaximumNumberOfCoefficients = 50;
                    break;
            }

            _nodePotential.SingularityDetection = nodePotentialSingularityDetection;
            _currentIteration.TargetPrecision = currentIterationTargetPrecision;
            _currentIteration.MaximumIterations = currentIterationMaximumIterations;
            _newtonRaphson.TargetPrecision = newtonRaphsonTargetPrecision;
            _newtonRaphson.MaximumIterations = newtonRaphsonMaximumIterations;
            _fastDecoupledLoadFlow.TargetPrecision = fdlfTargetPrecision;
            _fastDecoupledLoadFlow.MaximumIterations = fdlfMaximumIterations;
            _holomorphicEmbeddedLoadFlow.TargetPrecision = helmTargetPrecision;
            _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients = helmMaximumNumberOfCoefficients;
            _holomorphicEmbeddedLoadFlowHighAccuracy.TargetPrecision = helmTargetPrecision;
            _holomorphicEmbeddedLoadFlowHighAccuracy.MaximumNumberOfCoefficients = helmMaximumNumberOfCoefficients;
        }

        private void CalculateClicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(Directory.GetCurrentDirectory());
            _generalSettings.CalculationRunning = true;
            _calculationResults.Clear();
            var dispatcher = Dispatcher.CurrentDispatcher;
            CalculateProgressBar.Maximum = 6*_generalSettings.NumberOfExecutions;
            CalculateProgressBar.Value = 0;
            var calculationTask = Task.Factory.StartNew(() =>
            {
                CalculateNodePotentialResult(dispatcher);
                CalculateCurrentIterationResult(dispatcher);
                CalculateNewtonRaphsonResult(dispatcher);
                CalculateFastDecoupledLoadFlowResult(dispatcher);
                CalculateHolomorphicEmbeddingLoadFlowResult(dispatcher);
                CalculateHolomorphicEmbeddingLoadFlowHighAccuracyResult(dispatcher);
            });

            calculationTask.ContinueWith(t => CalculationFinished(), CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CalculationFinished()
        {
            _generalSettings.CalculationRunning = false;
        }

        private void CalculateNodePotentialResult(Dispatcher mainDispatcher)
        {
            var calculator = new NodePotentialMethod(_nodePotential.SingularityDetection);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "Node Potential";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private void CalculateCurrentIterationResult(Dispatcher mainDispatcher)
        {
            var calculator = new CurrentIteration(_currentIteration.TargetPrecision,
                _currentIteration.MaximumIterations);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "Current Iteration";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private void CalculateNewtonRaphsonResult(Dispatcher mainDispatcher)
        {
            var calculator = new NewtonRaphsonMethod(_newtonRaphson.TargetPrecision,
                _newtonRaphson.MaximumIterations);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "Newton Raphson";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private void CalculateFastDecoupledLoadFlowResult(Dispatcher mainDispatcher)
        {
            var calculator = new FastDecoupledLoadFlowMethod(_fastDecoupledLoadFlow.TargetPrecision,
                _fastDecoupledLoadFlow.MaximumIterations);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "FDLF";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private void CalculateHolomorphicEmbeddingLoadFlowResult(Dispatcher mainDispatcher)
        {
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(_holomorphicEmbeddedLoadFlow.TargetPrecision,
                _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients, DataType.LongDouble);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "HELM - double";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private void CalculateHolomorphicEmbeddingLoadFlowHighAccuracyResult(Dispatcher mainDispatcher)
        {
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(_holomorphicEmbeddedLoadFlow.TargetPrecision,
                _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients, DataType.MultiPrecision);
            var result = CalculateResult(calculator, mainDispatcher);
            result.Algorithm = "HELM - multi";
            mainDispatcher.Invoke(new ResultCalculated(AddCalculationResult), result);
        }

        private CalculationResult CalculateResult(LoadFlowCalculator calculator, Dispatcher mainDispatcher)
        {
            var numberOfExecutions = _generalSettings.NumberOfExecutions;
            var executionTimes = new List<double>(numberOfExecutions);
            var stopWatch = new Stopwatch();
            PowerNetSingleVoltageLevel powerNet;
            bool voltageCollapseDetected;
            Vector<Complex> correctVoltages;
            bool voltageCollapseReal;
            var i = 0;

            do
            {
                powerNet = CreatePowerNet(out correctVoltages, out voltageCollapseReal);

                stopWatch.Start();
                voltageCollapseDetected = powerNet.CalculateMissingInformation(calculator);
                stopWatch.Stop();
                executionTimes.Add(stopWatch.Elapsed.TotalSeconds);

                ++i;
                mainDispatcher.Invoke(new ProblemOnceSolved(IncreaseProgressBarCount));
            } while (i < numberOfExecutions);

            var result = new CalculationResult();
            var statistics = new DescriptiveStatistics(executionTimes);
            var voltageError = correctVoltages - powerNet.NodeVoltages;
            result.VoltageCollapseDetected = voltageCollapseDetected;
            result.VoltageCollapse = voltageCollapseReal;
            result.AverageExecutionTime = statistics.Mean;
            result.StandardDeviationExecutionTime = statistics.StandardDeviation;
            result.RelativePowerError = powerNet.RelativePowerError;
            result.MaximumRelativeVoltageError = voltageError.AbsoluteMaximum().Magnitude/powerNet.NominalVoltage;

            return result;
        }

        private PowerNetSingleVoltageLevel CreatePowerNet(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            switch (_generalSettings.ProblemSelection)
            {
                case ProblemSelectionEnum.CollapsingTwoNodeSystem:
                    return CreatePowerNetForCollapsingTwoNodeSystem(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.StableTwoNodeSystem:
                    return CreatePowerNetForStableTwoNodeSystem(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithFourPQBuses:
                    return CreatePowerNetForFiveNodeSystemWithFourPQBuses(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithOneGroundNode:
                    return CreatePowerNetForFiveNodeSystemWithOneGroundNode(out correctVoltages, out voltageCollapse);
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    return CreatePowerNetForFiveNodeSystemWithThreePQBusesAndOnePVBus(out correctVoltages,
                        out voltageCollapse);
            }

            throw new ArgumentOutOfRangeException();
        }

        private PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithFourPQBuses(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.SetAdmittance(0, 1, new Complex(1000, 500));
            powerNet.SetAdmittance(0, 2, new Complex(0, 0));
            powerNet.SetAdmittance(0, 3, new Complex(200, -100));
            powerNet.SetAdmittance(0, 4, new Complex(0, -200));
            powerNet.SetAdmittance(1, 2, new Complex(100, 300));
            powerNet.SetAdmittance(1, 3, new Complex(0, 0));
            powerNet.SetAdmittance(1, 4, new Complex(0, 0));
            powerNet.SetAdmittance(2, 3, new Complex(200, -500));
            powerNet.SetAdmittance(2, 4, new Complex(0, 0));
            powerNet.SetAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(5);
            correctVoltages[0] = new Complex(1, -0.1);
            correctVoltages[1] = new Complex(1.05, 0.1);
            correctVoltages[2] = new Complex(0.95, 0.2);
            correctVoltages[3] = new Complex(0.97, -0.15);
            correctVoltages[4] = new Complex(0.95, 0.03);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);

            for (var i = 1; i < 5; ++i)
            {
                var node = new Node();
                node.Power = powers[i];
                powerNet.SetNode(i, node);
            }

            var supplyNode = new Node();
            supplyNode.Voltage = correctVoltages[0];
            powerNet.SetNode(0, supplyNode);

            voltageCollapse = false;
            return powerNet;
        }

        private PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithOneGroundNode(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.SetAdmittance(0, 1, new Complex(1000, 500));
            powerNet.SetAdmittance(0, 2, new Complex(0, 0));
            powerNet.SetAdmittance(0, 3, new Complex(200, -100));
            powerNet.SetAdmittance(0, 4, new Complex(0, -200));
            powerNet.SetAdmittance(1, 2, new Complex(100, 300));
            powerNet.SetAdmittance(1, 3, new Complex(0, 0));
            powerNet.SetAdmittance(1, 4, new Complex(0, 0));
            powerNet.SetAdmittance(2, 3, new Complex(200, -500));
            powerNet.SetAdmittance(2, 4, new Complex(0, 0));
            powerNet.SetAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0, 0) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            IList<Node> nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Power = powers.At(0);
            nodes[1].Voltage = correctVoltages.At(1);
            nodes[2].Voltage = correctVoltages.At(2);
            nodes[3].Power = powers.At(3);
            nodes[4].Voltage = correctVoltages.At(4);

            for (var i = 0; i < 5; ++i)
                powerNet.SetNode(i, nodes[i]);

            voltageCollapse = false;
            return powerNet;
        }

        private PowerNetSingleVoltageLevel CreatePowerNetForFiveNodeSystemWithThreePQBusesAndOnePVBus(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(5, 1);
            powerNet.SetAdmittance(0, 1, new Complex(1000, 500));
            powerNet.SetAdmittance(0, 2, new Complex(0, 0));
            powerNet.SetAdmittance(0, 3, new Complex(200, -100));
            powerNet.SetAdmittance(0, 4, new Complex(0, -200));
            powerNet.SetAdmittance(1, 2, new Complex(100, 300));
            powerNet.SetAdmittance(1, 3, new Complex(0, 0));
            powerNet.SetAdmittance(1, 4, new Complex(0, 0));
            powerNet.SetAdmittance(2, 3, new Complex(200, -500));
            powerNet.SetAdmittance(2, 4, new Complex(0, 0));
            powerNet.SetAdmittance(3, 4, new Complex(700, 500));
            correctVoltages = new DenseVector(new[] { new Complex(1, -0.1), new Complex(1.05, 0.1), new Complex(0.95, 0.2), new Complex(0.97, -0.15), new Complex(0, 0) });
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            IList<Node> nodes = new[] { new Node(), new Node(), new Node(), new Node(), new Node() };
            nodes[0].Voltage = correctVoltages.At(0);
            nodes[1].Power = powers.At(1);
            nodes[2].VoltageMagnitude = correctVoltages.At(2).Magnitude;
            nodes[2].RealPower = powers.At(2).Real;
            nodes[3].Power = powers.At(3);
            nodes[4].Power = powers.At(4);

            for (var i = 0; i < 5; ++i)
                powerNet.SetNode(i, nodes[i]);

            voltageCollapse = false;
            return powerNet;
        }

        private PowerNetSingleVoltageLevel CreatePowerNetForStableTwoNodeSystem(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            powerNet.SetAdmittance(0, 1, new Complex(1000, 2000));
            correctVoltages = new DenseVector(2);
            correctVoltages[0] = new Complex(1, 0);
            correctVoltages[1] = new Complex(0.95, 0.03);
            var powers = LoadFlowCalculator.CalculateAllPowers(powerNet.Admittances, correctVoltages);
            var supplyNode = new Node();
            var loadNode = new Node();
            supplyNode.Voltage = correctVoltages[0];
            loadNode.Power = powers[1];
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            voltageCollapse = false;
            return powerNet;
        }

        public static PowerNetSingleVoltageLevel CreatePowerNetForCollapsingTwoNodeSystem(out Vector<Complex> correctVoltages, out bool voltageCollapse)
        {
            var powerNet = new PowerNetSingleVoltageLevel(2, 1);
            var supplyNode = new Node();
            supplyNode.Voltage = new Complex(1, 0);
            var loadNode = new Node();
            loadNode.Power = new Complex(-100, -50);
            powerNet.SetNode(0, supplyNode);
            powerNet.SetNode(1, loadNode);
            powerNet.SetAdmittance(0, 1, new Complex(1, 2));

            correctVoltages = new DenseVector(2);
            voltageCollapse = true;
            return powerNet;
        }
    }
}
