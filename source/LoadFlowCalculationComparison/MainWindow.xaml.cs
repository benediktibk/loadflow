using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using LoadFlowCalculationComparison.AlgorithmSettings;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.Statistics;

namespace LoadFlowCalculationComparison
{
    public partial class MainWindow
    {
        private readonly CurrentIterationSettings _currentIteration;
        private readonly FastDecoupledLoadFlowMethodSettings _fastDecoupledLoadFlow;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlow;
        private readonly NewtonRaphsonMethodSettings _newtonRaphson;
        private readonly NodePotentialMethodSettings _nodePotential;
        private readonly GeneralSettings _generalSettings;
        private readonly ObservableCollection<CalculationResult> _calculationResults;

        public MainWindow()
        {
            _currentIteration = new CurrentIterationSettings();
            _fastDecoupledLoadFlow = new FastDecoupledLoadFlowMethodSettings();
            _holomorphicEmbeddedLoadFlow = new HolomorphicEmbeddedLoadFlowMethodSettings();
            _newtonRaphson = new NewtonRaphsonMethodSettings();
            _nodePotential = new NodePotentialMethodSettings();
            _generalSettings = new GeneralSettings();
            _calculationResults = new ObservableCollection<CalculationResult>(new List<CalculationResult>(10));
            
            InitializeComponent();
            NodePotentialGrid.DataContext = _nodePotential;
            HolomorphicEmbeddedLoadFlowGrid.DataContext = _holomorphicEmbeddedLoadFlow;
            FastDecoupledLoadFlowGrid.DataContext = _fastDecoupledLoadFlow;
            CurrentIterationGrid.DataContext = _currentIteration;
            NewtonRaphsonGrid.DataContext = _newtonRaphson;
            GeneralSettingsGrid.DataContext = _generalSettings;
            CalculateButton.DataContext = _generalSettings;
            ResultDataGrid.ItemsSource = _calculationResults;
        }

        private void ProblemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double nodePotentialSingularityDetection = 0;
            double currentIterationTerminationCriteria = 0;
            int currentIterationMaximumIterations = 0;
            double newtonRaphsonTargetPrecision = 0;
            int newtonRaphsonMaximumIterations = 0;
            double fdlfTargetPrecision = 0;
            int fdlfMaximumIterations = 0;
            double helmTargetPrecision = 0;
            int helmMaximumNumberOfCoefficients = 0;

            switch (_generalSettings.ProblemSelection)
            {
                case ProblemSelectionEnum.CollapsingTwoNodeSystem:
                case ProblemSelectionEnum.StableTwoNodeSystem:
                    nodePotentialSingularityDetection = 0.00001;
                    currentIterationTerminationCriteria = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmTargetPrecision = 0.00001;
                    helmMaximumNumberOfCoefficients = 50;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithFourPQBuses:
                    throw new NotImplementedException();
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    throw new NotImplementedException();
            }

            _nodePotential.SingularityDetection = nodePotentialSingularityDetection;
            _currentIteration.TerminationCriteria = currentIterationTerminationCriteria;
            _currentIteration.MaximumIterations = currentIterationMaximumIterations;
            _newtonRaphson.TargetPrecision = newtonRaphsonTargetPrecision;
            _newtonRaphson.MaximumIterations = newtonRaphsonMaximumIterations;
            _fastDecoupledLoadFlow.TargetPrecision = fdlfTargetPrecision;
            _fastDecoupledLoadFlow.MaximumIterations = fdlfMaximumIterations;
            _holomorphicEmbeddedLoadFlow.TargetPrecision = helmTargetPrecision;
            _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients = helmMaximumNumberOfCoefficients;
        }

        private void CalculateClicked(object sender, RoutedEventArgs e)
        {
            _calculationResults.Clear();
            _calculationResults.Add(CalculateNodePotentialResult());
            _calculationResults.Add(CalculateCurrentIterationResult());
            _calculationResults.Add(CalculateNewtonRaphsonResult());
            _calculationResults.Add(CalculateFastDecoupledLoadFlowResult());
            _calculationResults.Add(CalculateHolomorphicEmbeddingLoadFlowResult());
            
        }

        private CalculationResult CalculateNodePotentialResult()
        {
            var calculator = new NodePotentialMethod(_nodePotential.SingularityDetection);
            var result = CalculateResult(calculator);
            result.Algorithm = "Node Potential Method";
            return result;
        }

        private CalculationResult CalculateCurrentIterationResult()
        {
            var calculator = new CurrentIteration(_currentIteration.TerminationCriteria,
                _currentIteration.MaximumIterations);
            var result = CalculateResult(calculator);
            result.Algorithm = "Current Iteration";
            return result;
        }

        private CalculationResult CalculateNewtonRaphsonResult()
        {
            var calculator = new NewtonRaphsonMethod(_newtonRaphson.TargetPrecision,
                _newtonRaphson.MaximumIterations);
            var result = CalculateResult(calculator);
            result.Algorithm = "Newton Raphson Method";
            return result;
        }

        private CalculationResult CalculateFastDecoupledLoadFlowResult()
        {
            var calculator = new FastDecoupledLoadFlowMethod(_fastDecoupledLoadFlow.TargetPrecision,
                _fastDecoupledLoadFlow.MaximumIterations);
            var result = CalculateResult(calculator);
            result.Algorithm = "Fast Decoupled Load Flow Method";
            return result;
        }

        private CalculationResult CalculateHolomorphicEmbeddingLoadFlowResult()
        {
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(_holomorphicEmbeddedLoadFlow.TargetPrecision,
                _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients, false);
            var result = CalculateResult(calculator);
            result.Algorithm = "Holomorphic Embedding Load Flow Method";
            return result;
        }

        private CalculationResult CalculateResult(LoadFlowCalculator calculator)
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
            } while (i < numberOfExecutions);

            var result = new CalculationResult();
            result.VoltageCollapseDetected = voltageCollapseDetected;
            result.VoltageCollapse = voltageCollapseReal;

            if (voltageCollapseDetected) 
                return result;

            var voltageError = correctVoltages - powerNet.NodeVoltages;
            var statistics = new DescriptiveStatistics(executionTimes);
            result.RelativePowerError = powerNet.RelativePowerError;
            result.MaximumRelativeVoltageError = voltageError.AbsoluteMaximum().Magnitude/powerNet.NominalVoltage;
            result.AverageExecutionTime = statistics.Mean;
            result.StandardDeviationExecutionTime = statistics.StandardDeviation;

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
                    throw new ArgumentOutOfRangeException();
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    throw new ArgumentOutOfRangeException();
            }

            throw new ArgumentOutOfRangeException();
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
