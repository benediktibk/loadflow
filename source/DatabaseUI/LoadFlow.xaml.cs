using System;
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

            if (_model == null)
                throw new Exception("resource is missing");

            _converter = FindResource("NodeToNodeNameConverter") as NodeToNodeNameConverter;

            if (_converter == null)
                throw new Exception("resource is missing");

            _model.SelectedPowerNetChanged += UpdateNodeToNodeNameConverter;
            _model.SelectedPowerNetChanged += UpdateSubscriptionOfNodesChanged;

            var powerNetOne = new PowerNet();
            var powerNetTwo = new PowerNet();
            powerNetOne.Nodes.Add(new Node { Name = "blub" });
            powerNetOne.Nodes.Add(new Node { Name = "blob" });
            powerNetTwo.Nodes.Add(new Node { Name = "heinz" });
            powerNetTwo.Nodes.Add(new Node { Name = "hanz" });
            powerNetTwo.Nodes.Add(new Node { Name = "kunz" });

            _model.PowerNets.Add(powerNetOne);
            _model.PowerNets.Add(powerNetTwo);
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
            throw new NotImplementedException();
        }
    }
}
