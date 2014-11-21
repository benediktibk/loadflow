using System;
using System.Diagnostics;
using System.Windows;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.Win32;

namespace SincalConnector
{
    public partial class MainWindow
    {
        private readonly ConnectorData _connectorData;

        public MainWindow()
        {
            InitializeComponent();

            _connectorData = FindResource("ConnectorData") as ConnectorData;

            if (_connectorData == null)
                throw new Exception("could not find static resource");

            _connectorData.Log("application started");
        }

        private void OpenFileDialogForInputPowerNet(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "MS Access Database (.mdb)|*.mdb|All Files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            var userClickedOK = openFileDialog.ShowDialog();

            if (userClickedOK != true)
                return;

            _connectorData.InputFile = openFileDialog.FileName;
        }

        private void CalculatePowerNet(object sender, RoutedEventArgs e)
        {
            PowerNetDatabaseAdapter powerNet;

            try
            {
                _connectorData.Log("parsing the power net");
                powerNet = new PowerNetDatabaseAdapter(_connectorData.InputFile);
            }
            catch (Exception exception)
            {
                _connectorData.Log("could not open selected file " + _connectorData.InputFile);
                _connectorData.Log(exception.Message);
                return;
            }

            var calculator = _connectorData.CreateCalculator();
            var stopWatch = new Stopwatch();

            try
            {
                _connectorData.Log("calculating the power net");
                double relativePowerError;
                stopWatch.Start();
                var success = powerNet.CalculateNodeVoltages(calculator, out relativePowerError);
                stopWatch.Stop();
                _connectorData.Log(success ? 
                    "finished calculation of power net after " + stopWatch.Elapsed.TotalSeconds + "s with a relative power mismatch of " + relativePowerError : 
                    "was not able to calculate the power net");

            }
            catch (Exception exception)
            {
                _connectorData.Log("was not able to calculate the power net because an exception occured:");
                _connectorData.Log(exception.Message);
            }
        }
    }
}
