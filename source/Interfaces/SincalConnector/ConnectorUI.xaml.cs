using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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

        private void CalculateClicked(object sender, RoutedEventArgs e)
        {
            _calculationThread.CalculatePowerNet();
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
