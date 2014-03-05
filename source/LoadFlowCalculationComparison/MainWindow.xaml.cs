using System.Windows.Controls;
using LoadFlowCalculationComparison.AlgorithmSettings;

namespace LoadFlowCalculationComparison
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly CurrentIterationSettings _currentIteration;
        private readonly FastDecoupledLoadFlowMethodSettings _fastDecoupledLoadFlow;
        private readonly HolomorphicEmbeddedLoadFlowMethodSettings _holomorphicEmbeddedLoadFlow;
        private readonly NewtonRaphsonMethodSettings _newtonRaphson;
        private readonly NodePotentialMethodSettings _nodePotential;

        public MainWindow()
        {
            _currentIteration = new CurrentIterationSettings();
            _fastDecoupledLoadFlow = new FastDecoupledLoadFlowMethodSettings();
            _holomorphicEmbeddedLoadFlow = new HolomorphicEmbeddedLoadFlowMethodSettings();
            _newtonRaphson = new NewtonRaphsonMethodSettings();
            _nodePotential = new NodePotentialMethodSettings();
            
            InitializeComponent();
            NodePotentialGrid.DataContext = _nodePotential;
            HolomorphicEmbeddedLoadFlowGrid.DataContext = _holomorphicEmbeddedLoadFlow;
            FastDecoupledLoadFlowGrid.DataContext = _fastDecoupledLoadFlow;
            CurrentIterationGrid.DataContext = _currentIteration;
            NewtonRaphsonGrid.DataContext = _newtonRaphson;
        }

        private void ProblemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _nodePotential.SingularityDetection = 1;
        }
    }
}
