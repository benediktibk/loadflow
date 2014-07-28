using System;
using System.Windows;
using Database;

namespace DatabaseUI
{
    public partial class MainWindow
    {
        #region variables

        private readonly Model _model;
        private readonly NodeToNodeNameConverter _converter;

        #endregion

        #region constructor

        public MainWindow()
        {
            InitializeComponent();
            _model = FindResource("Database") as Model;
            _converter = FindResource("NodeToNodeNameConverter") as NodeToNodeNameConverter;

            if (_model == null || _converter == null)
                throw new Exception("resource is missing");

            _model.SelectedPowerNetChanged += UpdateNodeToNodeNameConverter;
            _model.SelectedPowerNetChanged += UpdateSubscriptionOfNodesChanged;
            LoggingOutput.TextChanged += ScrollLoggingOutputToEnd;

            CreateDefaultPowerNets();
        }

        #endregion

        #region private functions

        private void CreateDefaultPowerNets()
        {
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
            powerNetThree.Loads.Add(new Load { Node = powerNetThree.Nodes[1], Real = -2000, Imaginary = 0 });
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

        private void UpdateNodeToNodeNameConverter()
        {
            _converter.UpdateMapping(_model.SelectedPowerNet.Nodes);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            using (var database = new DatabaseContext(DatabaseConnectionString.Text))
            {
                database.PowerNets.AddRange(_model.PowerNets);
                database.SaveChanges();
            }
        }

        private void CalculateNodeVoltages(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedPowerNet == null)
                return;

            _model.SelectedPowerNet.CalculateNodeVoltagesInBackground();
        }

        private void ScrollLoggingOutputToEnd(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LoggingOutput.ScrollToEnd();
        }

        #endregion
    }
}
