using System;
using System.Windows;
using System.Windows.Controls;
using LoadFlowCalculationComparison.AlgorithmSettings;

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

        public MainWindow()
        {
            _currentIteration = new CurrentIterationSettings();
            _fastDecoupledLoadFlow = new FastDecoupledLoadFlowMethodSettings();
            _holomorphicEmbeddedLoadFlow = new HolomorphicEmbeddedLoadFlowMethodSettings();
            _newtonRaphson = new NewtonRaphsonMethodSettings();
            _nodePotential = new NodePotentialMethodSettings();
            _generalSettings = new GeneralSettings();
            
            InitializeComponent();
            NodePotentialGrid.DataContext = _nodePotential;
            HolomorphicEmbeddedLoadFlowGrid.DataContext = _holomorphicEmbeddedLoadFlow;
            FastDecoupledLoadFlowGrid.DataContext = _fastDecoupledLoadFlow;
            CurrentIterationGrid.DataContext = _currentIteration;
            NewtonRaphsonGrid.DataContext = _newtonRaphson;
            GeneralSettingsGrid.DataContext = _generalSettings;
            CalculateButton.DataContext = _generalSettings;
        }

        private void ProblemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double nodePotentialSingularityDetection;
            double currentIterationTerminationCriteria;
            int currentIterationMaximumIterations;
            double newtonRaphsonTargetPrecision;
            int newtonRaphsonMaximumIterations;
            double fdlfTargetPrecision;
            int fdlfMaximumIterations;
            double helmTargetPrecision;
            int helmMaximumNumberOfCoefficients;

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
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
        }
    }
}
