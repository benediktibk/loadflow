using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Database;
using Node = Database.Node;

namespace DatabaseUI
{
    public partial class MainWindow
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
            LoggingOutput.TextChanged += ScrollLoggingOutputToEnd;
            Closed += DisposeConnection;
        }

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

        private void CalculateAdmittanceMatrix(object sender, RoutedEventArgs e)
        {
            var powerNet = _model.SelectedPowerNet;

            if (powerNet == null || _model.Connection.NotConnected)
                return;

            Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix matrix;
            IReadOnlyList<string> nodeNames;
            double powerBase;

            powerNet.Log("calculating the admittance matrix");
            powerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);

            if (matrix == null)
                return;

            powerNet.Log("storing the admittance matrix in the database");
            _model.Connection.Add(matrix, nodeNames, powerNet.Name, powerBase);
            powerNet.Log("done");
        }

        private void ScrollLoggingOutputToEnd(object sender, TextChangedEventArgs e)
        {
            LoggingOutput.ScrollToEnd();
        }

        private void ToggleConnect(object sender, RoutedEventArgs e)
        {
            if (_model.Connection.Connected)
                DisconnectAndClear();
            else
                TryToConnect();
        }

        private void DisconnectAndClear()
        {
            _model.Connection.Disconnect();
            _model.Clear();
        }

        private void TryToConnect()
        {
            try
            {
                _model.Connection.Connect();
            }
            catch (Exception connectException)
            {
                var dialogResult =
                    MessageBox.Show("An error occured: " + connectException.Message + "\r\n\r\nTry to create database?", "error",
                        MessageBoxButton.YesNo);

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
                _model.Connection.Connect();
            }

            _model.ReadFromDatabase();
        }

        private void CheckIfNodeCanBeDeleted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command != DataGrid.DeleteCommand)
                return;

            var nodes = NodesDataGrid.SelectedItems.Cast<Node>();

            foreach (var node in nodes)
            {
                if (!_model.SelectedPowerNet.IsNodeInUse(node))
                    continue;

                MessageBox.Show("The node " + node.Name + " can not be deleted as it is still in use.",
                    "deleting node", MessageBoxButton.OK);
                e.Handled = true;
                return;
            }
        }

        private void DisposeConnection(object sender, EventArgs e)
        {
            _model.Connection.Dispose();
        }
    }
}
