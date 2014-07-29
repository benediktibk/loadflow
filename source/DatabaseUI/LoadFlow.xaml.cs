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
        }

        #endregion

        #region private functions

        private void UpdateSubscriptionOfNodesChanged()
        {
            if (_model.SelectedPowerNet == null)
                return;

            _model.SelectedPowerNet.NodesChanged += UpdateNodeToNodeNameConverter;
        }

        private void UpdateNodeToNodeNameConverter()
        {
            if (_model.SelectedPowerNet == null)
                return;

            _converter.UpdateMapping(_model.SelectedPowerNet.Nodes);
        }

        private void CalculateNodeVoltages(object sender, RoutedEventArgs e)
        {
            if (_model.SelectedPowerNet == null || _model.Connection.NotConnected)
                return;

            _model.SelectedPowerNet.CalculateNodeVoltagesInBackground();
        }

        private void ScrollLoggingOutputToEnd(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            LoggingOutput.ScrollToEnd();
        }

        private void ToggleConnect(object sender, RoutedEventArgs e)
        {
            try
            {
                _model.Connection.ToggleConnect();
            }
            catch (Exception connectException)
            {
                var dialogResult = MessageBox.Show("An error occured: " + connectException.Message + "\r\n\r\nTry to create database?", "error", MessageBoxButton.YesNo);

                if (dialogResult != MessageBoxResult.Yes)
                    return;

                try
                {
                    _model.Connection.CreateDatabase();
                }
                catch (Exception creationException)
                {
                    MessageBox.Show("creating the database failed: " + creationException.Message);
                    return;
                }

                System.Threading.Thread.Sleep(5000);
                ToggleConnect(null, null);
            }

            _model.ReadFromDatabase();
        }

        #endregion
    }
}
