﻿using System.Collections.Generic;
using System.ComponentModel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SincalConnector;

namespace SincalConnectorTest
{
    [TestClass]
    public class ConnectorDataTest
    {
        private ConnectorData _connectorData;
        private List<string> _changedProperties;

        [TestInitialize]
        public void SetUp()
        {
            _connectorData = new ConnectorData();
            _connectorData.PropertyChanged += ConnectorDataPropertyChanged;
            _changedProperties = new List<string>();
        }

        void ConnectorDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _changedProperties.Add(e.PropertyName);
        }

        [TestMethod]
        public void Constructor_Empty_IsValid()
        {
            Assert.IsTrue(_connectorData.IsValid);
        }

        [TestMethod]
        public void Constructor_Empty_ReasonableInitialValues()
        {
            Assert.AreEqual(64, _connectorData.SizeOfDataType);
            Assert.IsTrue(_connectorData.CountOfCoefficients > 30);
            Assert.IsTrue(_connectorData.MaximumIterations > 0);
            Assert.IsTrue(_connectorData.TargetPrecision < 1e-3);
            Assert.IsTrue(_connectorData.Progress >= 0);
            Assert.IsTrue(_connectorData.Progress <= 1);
            Assert.IsTrue(_connectorData.TotalProgress >= 0);
            Assert.IsTrue(_connectorData.TotalProgress <= 1);
            Assert.AreEqual(Selection.HolomorphicEmbeddedLoadFlow, _connectorData.CalculatorSelection);
            Assert.IsTrue(_connectorData.IterativeSolver);
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
        public void IsValid_NegativeMaximumIterations_False()
        {
            _connectorData.MaximumIterations = -1;

            Assert.IsFalse(_connectorData.IsValid);
        }

        [TestMethod]
        public void IsValid_TargetPrecisionSetTo1_False()
        {
            _connectorData.TargetPrecision = 1;

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

        [TestMethod]
        public void TargetPrecision_NewValueSet_CorrectValueSet()
        {
            _connectorData.TargetPrecision = 0.1234;

            Assert.AreEqual(0.1234, _connectorData.TargetPrecision, 1e-10);
        }

        [TestMethod]
        public void TargetPrecision_NewValueSet_PropertyTargetPrecisionChanged()
        {
            _connectorData.TargetPrecision = 0.1234;

            Assert.IsTrue(_changedProperties.Contains("TargetPrecision"));
        }

        [TestMethod]
        public void TargetPrecision_NewValueSet_PropertyIsValidChanged()
        {
            _connectorData.TargetPrecision = 0.1234;

            Assert.IsTrue(_changedProperties.Contains("IsValid"));
        }

        [TestMethod]
        public void MaximumIterations_NewValueSet_CorrectValueSet()
        {
            _connectorData.MaximumIterations = 1234;

            Assert.AreEqual(1234, _connectorData.MaximumIterations);
        }

        [TestMethod]
        public void MaximumIterations_NewValueSet_PropertyMaximumIterationsChanged()
        {
            _connectorData.MaximumIterations = 1234;

            Assert.IsTrue(_changedProperties.Contains("MaximumIterations"));
        }

        [TestMethod]
        public void MaximumIterations_NewValueSet_PropertyIsValidChanged()
        {
            _connectorData.MaximumIterations = 1234;

            Assert.IsTrue(_changedProperties.Contains("IsValid"));
        }

        [TestMethod]
        public void CalculatorSelection_NewValueSet_CorrectValueSet()
        {
            _connectorData.CalculatorSelection = Selection.NodePotential;

            Assert.AreEqual(Selection.NodePotential, _connectorData.CalculatorSelection);
        }

        [TestMethod]
        public void CalculatorSelection_NewValueSet_PropertyCalculatorSelectionChanged()
        {
            _connectorData.CalculatorSelection = Selection.NodePotential;

            Assert.IsTrue(_changedProperties.Contains("CalculatorSelection"));
        }

        [TestMethod]
        public void Progress_NewValueSet_CorrectValue()
        {
            _connectorData.Progress = 0.156465;

            Assert.AreEqual(0.156465, _connectorData.Progress, 0.000000001);
        }

        [TestMethod]
        public void Progress_NewValueSet_PropertyProgressChanged()
        {
            _connectorData.Progress = 0.156465;

            Assert.IsTrue(_changedProperties.Contains("Progress"));
        }

        [TestMethod]
        public void TotalProgress_NewValueSet_CorrectValue()
        {
            _connectorData.TotalProgress = 0.156465;

            Assert.AreEqual(0.156465, _connectorData.TotalProgress, 0.000000001);
        }

        [TestMethod]
        public void TotalProgress_NewValueSet_PropertyProgressChanged()
        {
            _connectorData.TotalProgress = 0.156465;

            Assert.IsTrue(_changedProperties.Contains("TotalProgress"));
        }

        [TestMethod]
        public void RelativePowerError_NewValueSet_CorrectValue()
        {
            _connectorData.RelativePowerError = 0.156465;

            Assert.AreEqual(0.156465, _connectorData.RelativePowerError, 0.000000001);
        }

        [TestMethod]
        public void RelativePowerError_NewValueSet_PropertyProgressChanged()
        {
            _connectorData.RelativePowerError = 0.156465;

            Assert.IsTrue(_changedProperties.Contains("RelativePowerError"));
        }

        [TestMethod]
        public void IterativeSolver_NewValueSet_CorrectValue()
        {
            _connectorData.IterativeSolver = false;

            Assert.IsFalse(_connectorData.IterativeSolver);
        }

        [TestMethod]
        public void IterativeSolver_NewValueSet_PropertyIterativeSolverChanged()
        {
            _connectorData.IterativeSolver = false;

            Assert.IsTrue(_changedProperties.Contains("IterativeSolver"));
        }
    }
}
