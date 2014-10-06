using System;
using System.Windows;
using Microsoft.Win32;

namespace SincalConnector
{
    public partial class MainWindow
    {
        #region variables

        private ConnectorData _connectorData;
        
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _connectorData = FindResource("ConnectorData") as ConnectorData;

            if (_connectorData == null)
                throw new Exception("could not find static resource");

            Log("starting application");
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
            throw new NotImplementedException();
        }

        private void Log(string message)
        {
            _connectorData.LoggingOutput += message + "\n";
        }
    }
}
