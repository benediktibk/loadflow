using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Microsoft.Win32;

namespace SincalConnector
{
    public partial class MainWindow
    {
        private readonly ConnectorData _connectorData;
        private readonly DispatcherTimer _progressTimer;
        private readonly CalculationThread _calculationThread;

        public MainWindow()
        {
            InitializeComponent();
            _progressTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 0, 1)};
            _progressTimer.Tick += RefreshProgress;
            _progressTimer.Start();
            _connectorData = FindResource("ConnectorData") as ConnectorData;
            _calculationThread = new CalculationThread(_connectorData);
            _calculationThread.Log += LogThreadSafe;

            if (_connectorData == null)
                throw new Exception("could not find static resource");

            Log += _connectorData.Log;
            Log("application started");
        }

        private void RefreshProgress(object sender, EventArgs eventArgs)
        {
            _connectorData.Progress = _calculationThread.Progress;
            _connectorData.TotalProgress = _calculationThread.TotalProgress;
            _connectorData.RelativePowerError = _calculationThread.RelativePowerError;
            _progressTimer.Start();
        }

        private delegate void LogHandler(string message);

        private event LogHandler Log;

        private void OpenFileDialogForInputPowerNet(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "MS Access Database (.mdb)|*.mdb|All Files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false
            };

            var userClickedOk = openFileDialog.ShowDialog();

            if (userClickedOk != true)
                return;

            _connectorData.InputFile = openFileDialog.FileName;
        }

        private void CalculateVoltagesClicked(object sender, RoutedEventArgs e)
        {
            _calculationThread.CalculatePowerNet();
        }

        private void CalculateAdmittancesClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var powerNet = new PowerNetDatabaseAdapter(_connectorData.InputFile, 1);
                AdmittanceMatrix matrix;
                IReadOnlyList<string> nodeNames;
                double powerBase;
                powerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);

                var singleVoltageMatrix = matrix.SingleVoltageAdmittanceMatrix;

                using (var file = new System.IO.StreamWriter(@"admittances.csv"))
                    foreach (var entry in singleVoltageMatrix.EnumerateIndexed())
                        file.WriteLine(entry.Item1 + ";" + entry.Item2 + ";" + entry.Item3.Real + ";" + entry.Item3.Imaginary);

                using (var file = new System.IO.StreamWriter(@"nodeNames.csv"))
                    foreach (var entry in nodeNames)
                        file.WriteLine(entry);

                using (var file = new System.IO.StreamWriter(@"scaling.csv"))
                    file.WriteLine("power base [W];" + powerBase);

                LogThreadSafe("files created");
            }
            catch (Exception exception)
            {
                LogThreadSafe(exception.Message);
            }
        }

        private void LogThreadSafe(string message)
        {
            if (Log == null)
                throw new NoNullAllowedException();

            if (Dispatcher.CheckAccess())
                Log(message);
            else
                Dispatcher.Invoke(Log, message);
        }

        private void LogTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = e.Source as TextBox;

            if (textBox == null)
                throw new Exception("could not find text box");

            textBox.ScrollToEnd();
        }
    }
}
