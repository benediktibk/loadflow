using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.Win32;

namespace SincalConnector
{
    public partial class MainWindow
    {
        private readonly ConnectorData _connectorData;
        private bool _isCalculationRunning;
        private readonly Mutex _isCalculationRunningMutex;
        private readonly BackgroundWorker _backgroundWorker;
        private PowerNetDatabaseAdapter _powerNet;
        private INodeVoltageCalculator _calculator;

        public MainWindow()
        {
            InitializeComponent();
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += CalculatePowerNetInternal;
            _backgroundWorker.RunWorkerCompleted += CalculationFinished;
            _connectorData = FindResource("ConnectorData") as ConnectorData;

            if (_connectorData == null)
                throw new Exception("could not find static resource");

            Log += _connectorData.Log;
            Log("application started");
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

        private void CalculatePowerNet(object sender, RoutedEventArgs e)
        {
            _isCalculationRunningMutex.WaitOne();
            if (_isCalculationRunning)
            {
                _isCalculationRunningMutex.ReleaseMutex();
                Log("calculation already running");
                return;
            }
            _isCalculationRunning = true;
            _isCalculationRunningMutex.ReleaseMutex();

            try
            {
                Log("parsing the power net");
                _powerNet = new PowerNetDatabaseAdapter(_connectorData.InputFile);
            }
            catch (Exception exception)
            {
                Log("could not open selected file " + _connectorData.InputFile);
                Log(exception.Message);
                _isCalculationRunningMutex.WaitOne();
                _isCalculationRunning = false;
                _isCalculationRunningMutex.ReleaseMutex();
                return;
            }

            _calculator = _connectorData.CreateCalculator();
            _backgroundWorker.RunWorkerAsync();
        }

        private void CalculationFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            _isCalculationRunningMutex.WaitOne();
            _isCalculationRunning = false;
            _isCalculationRunningMutex.ReleaseMutex();
        }

        private void CalculatePowerNetInternal(object sender, DoWorkEventArgs e)
        {
            var stopWatch = new Stopwatch();

            try
            {
                LogThreadSafe("calculating the power net");
                double relativePowerError;
                stopWatch.Start();
                var success = _powerNet.CalculateNodeVoltages(_calculator, out relativePowerError);
                stopWatch.Stop();
                LogThreadSafe(success ?
                    "finished calculation of power net after " + stopWatch.Elapsed.TotalSeconds + "s with a relative power mismatch of " + relativePowerError :
                    "was not able to calculate the power net");

            }
            catch (Exception exception)
            {
                LogThreadSafe("was not able to calculate the power net because an exception occured:");
                LogThreadSafe(exception.Message);
            }
        }

        private void LogThreadSafe(string message)
        {
            if (Dispatcher.CheckAccess())
                Log(message);
            else
                Dispatcher.Invoke(Log, new object[] { message });
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
