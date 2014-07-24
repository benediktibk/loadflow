using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CalculationComparison.AlgorithmSettings;

namespace CalculationComparison
{
    public partial class MainWindow
    {
        #region variables

        private readonly GeneralSettings _generalSettings;
        private readonly IterativeMethodSettings _currentIteration;
        private readonly IterativeMethodSettings _fastDecoupledLoadFlow;
        private readonly IterativeMethodSettings _newtonRaphson;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlow;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy _holomorphicEmbeddedLoadFlowHighAccuracy;
        private readonly CombinedCalculationResults _combinedCalculationResults;
        private readonly CalculationResults _calculationResults;
        private readonly NodeVoltages _nodeVoltages;
        private readonly Calculator _calculator;

        #endregion

        #region constructor

        public MainWindow()
        {
            _generalSettings = new GeneralSettings();
            _currentIteration = new IterativeMethodSettings(_generalSettings);
            _fastDecoupledLoadFlow = new IterativeMethodSettings(_generalSettings);
            _holomorphicEmbeddedLoadFlow = new HolomorphicEmbeddedLoadFlowMethodSettings(_generalSettings);
            _holomorphicEmbeddedLoadFlowHighAccuracy = new HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy(_generalSettings);
            _newtonRaphson = new IterativeMethodSettings(_generalSettings);

            InitializeComponent();
            _combinedCalculationResults = FindResource("CombinedCalculationResults") as CombinedCalculationResults;
            _calculationResults = FindResource("CalculationResults") as CalculationResults;
            _nodeVoltages = FindResource("NodeVoltages") as NodeVoltages;
            HolomorphicEmbeddedLoadFlowGrid.DataContext = _holomorphicEmbeddedLoadFlow;
            HolomorphicEmbeddedLoadFlowHighAccuracyGrid.DataContext = _holomorphicEmbeddedLoadFlowHighAccuracy;
            FastDecoupledLoadFlowGrid.DataContext = _fastDecoupledLoadFlow;
            CurrentIterationGrid.DataContext = _currentIteration;
            NewtonRaphsonGrid.DataContext = _newtonRaphson;
            GeneralSettingsGrid.DataContext = _generalSettings;
            CalculateButton.DataContext = _generalSettings;
            _calculationResults.CollectionChanged += CalculationResultsChanged;
            _calculator = new Calculator(
                _generalSettings, _currentIteration, _fastDecoupledLoadFlow, _newtonRaphson, _holomorphicEmbeddedLoadFlow, _holomorphicEmbeddedLoadFlowHighAccuracy, 
                _combinedCalculationResults, _calculationResults, _nodeVoltages, Dispatcher.CurrentDispatcher);
        }

        #endregion

        #region automatic gui update

        private void ScrollResultDataGridToEnd()
        {
            if (ResultDataGrid.Items.Count <= 0)
                return;

            var border = VisualTreeHelper.GetChild(ResultDataGrid, 0) as Decorator;
            if (border == null)
                return;

            var scroll = border.Child as ScrollViewer;
            if (scroll != null)
                scroll.ScrollToEnd();
        }

        private void ProblemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentIterationTargetPrecision = 0.0;
            var currentIterationMaximumIterations = 0;
            var newtonRaphsonTargetPrecision = 0.0;
            var newtonRaphsonMaximumIterations = 0;
            var fdlfTargetPrecision = 0.0;
            var fdlfMaximumIterations = 0;
            var helmLongDoubleTargetPrecision = 0.0;
            var helmLongDoubleMaximumNumberOfCoefficients = 0;
            var helmMultiTargetPrecision = 0.0;
            var helmMultiMaximumNumberOfCoefficients = 0;
            var helmMultiBitPrecision = 0;

            switch (_generalSettings.ProblemSelection)
            {
                case ProblemSelectionEnum.CollapsingTwoNodeSystem:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 50;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.StableTwoNodeSystem:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 10000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 50;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithFourPQBuses:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 10000;
                    helmLongDoubleTargetPrecision = 0.001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithOneGroundNode:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 10000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.FiveNodeSystemWithThreePQBusesAndOnePVBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 200;
                    helmMultiBitPrecision = 500;
                    break;
                case ProblemSelectionEnum.TwoNodeSystemWithOnePVBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.ThreeNodeSystemWithTwoPVBusses:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.NearlyCollapsingSystemWithPQBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 800;
                    helmMultiBitPrecision = 2000;
                    break;
                case ProblemSelectionEnum.NearlyCollapsingSystemWithPVBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.ThreeNodeSystemWithOnePQAndOnePVBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.00001;
                    newtonRaphsonMaximumIterations = 1000;
                    fdlfTargetPrecision = 0.00001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 60;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndOnePQAndOnePVBus:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 50;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndPVBusses:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 50;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
                case ProblemSelectionEnum.ThreeNodeSystemWithUnsymmetricMatrixAndPQBusses:
                    currentIterationTargetPrecision = 0.00001;
                    currentIterationMaximumIterations = 1000;
                    newtonRaphsonTargetPrecision = 0.001;
                    newtonRaphsonMaximumIterations = 2;
                    fdlfTargetPrecision = 0.000001;
                    fdlfMaximumIterations = 1000;
                    helmLongDoubleTargetPrecision = 0.00001;
                    helmLongDoubleMaximumNumberOfCoefficients = 50;
                    helmMultiTargetPrecision = 0.00001;
                    helmMultiMaximumNumberOfCoefficients = 100;
                    helmMultiBitPrecision = 300;
                    break;
            }

            _currentIteration.TargetPrecision = currentIterationTargetPrecision;
            _currentIteration.MaximumIterations = currentIterationMaximumIterations;
            _newtonRaphson.TargetPrecision = newtonRaphsonTargetPrecision;
            _newtonRaphson.MaximumIterations = newtonRaphsonMaximumIterations;
            _fastDecoupledLoadFlow.TargetPrecision = fdlfTargetPrecision;
            _fastDecoupledLoadFlow.MaximumIterations = fdlfMaximumIterations;
            _holomorphicEmbeddedLoadFlow.TargetPrecision = helmLongDoubleTargetPrecision;
            _holomorphicEmbeddedLoadFlow.MaximumNumberOfCoefficients = helmLongDoubleMaximumNumberOfCoefficients;
            _holomorphicEmbeddedLoadFlowHighAccuracy.TargetPrecision = helmMultiTargetPrecision;
            _holomorphicEmbeddedLoadFlowHighAccuracy.MaximumNumberOfCoefficients = helmMultiMaximumNumberOfCoefficients;
            _holomorphicEmbeddedLoadFlowHighAccuracy.BitPrecision = helmMultiBitPrecision;
        }

        void CalculationResultsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CalculateProgressBar.Value = _calculationResults.Count;
            ScrollResultDataGridToEnd();
        }

        #endregion

        #region result calculation

        private void CalculateClicked(object sender, RoutedEventArgs e)
        {
            CalculateProgressBar.Maximum = 6*_generalSettings.NumberOfExecutions;
            CalculateProgressBar.Value = 0;
            _calculator.Calculate();
        }

        #endregion
    }
}
