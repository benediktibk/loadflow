using System;
using System.ComponentModel;
using System.Windows;
using Database;

namespace DatabaseUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Model _model;
        private readonly NodeToNodeNameConverter _converter;

        public MainWindow()
        {
            InitializeComponent();
            _model = FindResource("Database") as Model;
            _converter = FindResource("NodeToNodeNameConverter") as NodeToNodeNameConverter;

            if (_model == null || _converter == null)
                throw new Exception("resource is missing");

            _model.SelectedPowerNetChanged += UpdateNodeToNodeNameConverter;
            _model.SelectedPowerNetChanged += UpdateSubscriptionOfNodesChanged;

            var powerNetOne = new PowerNet();
            var powerNetTwo = new PowerNet();
            var powerNetThree = new PowerNet();
            powerNetOne.Nodes.Add(new Node { Name = "blub" });
            powerNetOne.Nodes.Add(new Node { Name = "blob" });
            powerNetTwo.Nodes.Add(new Node { Name = "heinz" });
            powerNetTwo.Nodes.Add(new Node { Name = "hanz" });
            powerNetTwo.Nodes.Add(new Node { Name = "kunz" });
            powerNetThree.Nodes.Add(new Node { Id = 0, Name = "source", NominalVoltage = 400 });
            powerNetThree.Nodes.Add(new Node { Id = 1, Name = "target", NominalVoltage = 400 });
            powerNetThree.FeedIns.Add(new FeedIn
            {
                Node = powerNetThree.Nodes[0],
                ShortCircuitPower = 0,
                VoltageReal = 400,
                VoltageImaginary = 0
            });
            powerNetThree.Loads.Add(new Load {Node = powerNetThree.Nodes[1], Real = -2000, Imaginary = 0});
            powerNetThree.Lines.Add(new Line
            {
                Length = 1,
                NodeOne = powerNetThree.Nodes[0],
                NodeTwo = powerNetThree.Nodes[1],
                SeriesResistancePerUnitLength = 0.5
            });

            _model.PowerNets.Add(powerNetOne);
            _model.PowerNets.Add(powerNetTwo);
            _model.PowerNets.Add(powerNetThree);
        }

        private void UpdateSubscriptionOfNodesChanged()
        {
            _model.SelectedPowerNet.NodesChanged += UpdateNodeToNodeNameConverter;
        }

        void UpdateNodeToNodeNameConverter()
        {
            _converter.UpdateMapping(_model.SelectedPowerNet.Nodes);
        }

        private void Connect(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CalculateNodeVoltages(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedPowerNet == null)
                return;

            _model.SelectedPowerNet.CalculateNodeVoltagesInBackground();
        }
    }
}
