﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;

namespace SincalConnectorTest
{
    [TestClass]
    public class ConnectorDataTest
    {
        #region variables

        private ConnectorData _connectorData;
        private List<string> _changedProperties;

        #endregion

        #region set up

        [TestInitialize]
        public void SetUp()
        {
            _connectorData = new ConnectorData();
            _connectorData.PropertyChanged += ConnectorDataPropertyChanged;
            _changedProperties = new List<string>();
        }

        void ConnectorDataPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        #endregion

        #region tests

        [TestMethod]
        public void Constructor_Empty_IsValid()
        {
            Assert.IsTrue(_connectorData.IsValid);
        }

        [TestMethod]
        public void IsValid_NegativeSizeOfDataType_False()
        {
            _connectorData.SizeOfDataType = -1;

            Assert.IsFalse(_connectorData.IsValid);
        }

        [TestMethod]
        public void IsValid_NegativeCountOfCoefficients_False()
        {
            _connectorData.CountOfCoefficients = -1;

            Assert.IsFalse(_connectorData.IsValid);
        }

        [TestMethod]
        public void CountOfCoefficients_NewValueSet_PropertyChanged()
        {
            _connectorData.CountOfCoefficients = 156465;

            Assert.IsTrue(_changedProperties.Contains("CountOfCoefficients"));
        }

        [TestMethod]
        public void SizeOfDataType_NewValueSet_PropertyChanged()
        {
            _connectorData.SizeOfDataType = 156465;

            Assert.IsTrue(_changedProperties.Contains("SizeOfDataType"));
        }

        [TestMethod]
        public void CountOfCoefficients_NewValueSet_PropertyIsValidChanged()
        {
            _connectorData.CountOfCoefficients = 156465;

            Assert.IsTrue(_changedProperties.Contains("IsValid"));
        }

        [TestMethod]
        public void SizeOfDataType_NewValueSet_PropertyIsValidChanged()
        {
            _connectorData.SizeOfDataType = 156465;

            Assert.IsTrue(_changedProperties.Contains("IsValid"));
        }

        [TestMethod]
        public void InputFile_NewValueSet_PropertyChanged()
        {
            _connectorData.InputFile = "asdf";

            Assert.IsTrue(_changedProperties.Contains("InputFile"));
        }

        [TestMethod]
        public void Log_LineAdded_PropertyLoggingOutputChanged()
        {
            _connectorData.Log("asdf");

            Assert.IsTrue(_changedProperties.Contains("LoggingOutput"));
        }

        [TestMethod]
        public void Log_SeveralLinesAdded_LoggingOutputIsCorrect()
        {
            _connectorData.Log("asdf");
            _connectorData.Log("fd3");
            _connectorData.Log(" 34");

            Assert.AreEqual("asdf\nfd3\n 34\n", _connectorData.LoggingOutput);
        }

        #endregion
    }
}