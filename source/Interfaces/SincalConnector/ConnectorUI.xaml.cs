﻿using System;
using System.Windows;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.Win32;

namespace SincalConnector
{
    public partial class MainWindow
    {
        #region variables

        private readonly ConnectorData _connectorData;
        
        #endregion

        #region constructor

        public MainWindow()
        {
            InitializeComponent();

            _connectorData = FindResource("ConnectorData") as ConnectorData;

            if (_connectorData == null)
                throw new Exception("could not find static resource");

            _connectorData.Log("application started");
        }

        #endregion

        #region private functions

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
            PowerNet powerNet;

            try
            {
                _connectorData.Log("parsing the power net");
                powerNet = new PowerNet(_connectorData.InputFile);
            }
            catch (Exception exception)
            {
                _connectorData.Log("could not open selected file " + _connectorData.InputFile);
                _connectorData.Log(exception.Message);
                return;
            }

            var precision = Precision.Create(_connectorData.SizeOfDataType);
            var calculator = new HolomorphicEmbeddedLoadFlowMethod(0.000001, _connectorData.CountOfCoefficients,
                precision);

            _connectorData.Log("calculating the power net");
            var success = powerNet.CalculateNodeVoltages(calculator);

            _connectorData.Log(success ? "finished calculation of power net" : "was not able to calculate the power net");
        }

        #endregion
    }
}