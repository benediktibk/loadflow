﻿using System.ComponentModel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace SincalConnector
{
    public class ConnectorData : INotifyPropertyChanged, IReadOnlyConnectorData
    {
        private string _inputFile;
        private string _loggingOutput;
        private int _sizeOfDataType;
        private int _countOfCoefficients;
        private double _targetPrecision;
        private int _maximumIterations;
        private Selection _calculatorSelection;
        private double _progress;
        private double _totalProgress;
        private double _relativePowerError;
        private bool _iterativeSolver;

        public ConnectorData()
        {
            SizeOfDataType = 64;
            CountOfCoefficients = 50;
            MaximumIterations = 100;
            TargetPrecision = 1e-5;
            CalculatorSelection = Selection.HolomorphicEmbeddedLoadFlow;
            Progress = 0;
            TotalProgress = 0;
            RelativePowerError = 1;
            IterativeSolver = true;
        }

        public int SizeOfDataType
        {
            get { return _sizeOfDataType; }
            set
            {
                if (value == _sizeOfDataType) return;

                _sizeOfDataType = value;
                OnPropertyChanged("SizeOfDataType");
                OnPropertyChanged("IsValid");
            }
        }

        public int CountOfCoefficients
        {
            get { return _countOfCoefficients; }
            set
            {
                if (value == _countOfCoefficients) return;

                _countOfCoefficients = value;
                OnPropertyChanged("CountOfCoefficients");
                OnPropertyChanged("IsValid");
            }
        }

        public double TargetPrecision
        {
            get { return _targetPrecision; }
            set
            {
                if (value == _targetPrecision) return;

                _targetPrecision = value;
                OnPropertyChanged("TargetPrecision");
                OnPropertyChanged("IsValid");
            }
        }

        public int MaximumIterations
        {
            get { return _maximumIterations; }
            set
            {
                if (value == _maximumIterations) return;

                _maximumIterations = value;
                OnPropertyChanged("MaximumIterations");
                OnPropertyChanged("IsValid");
            }
        }

        public Selection CalculatorSelection
        {
            get { return _calculatorSelection; }
            set
            {
                if (value == _calculatorSelection) return;

                _calculatorSelection = value;
                OnPropertyChanged("CalculatorSelection");
            }
        }

        public double Progress
        {
            get { return _progress; }
            set
            {
                if (value == _progress) return;

                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        public double TotalProgress
        {
            get { return _totalProgress; }
            set
            {
                if (value == _totalProgress) return;

                _totalProgress = value;
                OnPropertyChanged("TotalProgress");
            }
        }

        public double RelativePowerError
        {
            get { return _relativePowerError; }
            set
            {
                if (value == _relativePowerError) return;

                _relativePowerError = value;
                OnPropertyChanged("RelativePowerError");
            }
        }

        public string InputFile
        {
            get { return _inputFile; }
            set
            {
                if (value == _inputFile) return;

                _inputFile = value;
                OnPropertyChanged("InputFile");
            }
        }

        public bool IterativeSolver
        {
            get { return _iterativeSolver; }
            set
            {
                if (value == _iterativeSolver) return;

                _iterativeSolver = value;
                OnPropertyChanged("IterativeSolver");
            }
        }

        public string LoggingOutput
        {
            get { return _loggingOutput; }
            private set
            {
                if (value == _loggingOutput) return;

                _loggingOutput = value;
                OnPropertyChanged("LoggingOutput");
            }
        }

        public bool IsValid
        {
            get { return SizeOfDataType > 0 && CountOfCoefficients > 2 && MaximumIterations > 0 && TargetPrecision > 0 && TargetPrecision < 1; }
        }

        public void Log(string message)
        {
            LoggingOutput += message + "\n";
        }

        public INodeVoltageCalculator CreateCalculator()
        {
            var factory = new Factory
            {
                BitPrecision = SizeOfDataType,
                CoefficientCount = CountOfCoefficients,
                MaximumIterations = MaximumIterations,
                TargetPrecision = TargetPrecision,
                IterativeSolver = IterativeSolver
            };
            return factory.CreateNodeVoltageCalculator(CalculatorSelection);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
