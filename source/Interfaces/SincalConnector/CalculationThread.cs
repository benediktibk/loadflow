using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace SincalConnector
{
    class CalculationThread
    {
        private bool _isCalculationRunning;
        private readonly Mutex _isCalculationRunningMutex;
        private readonly BackgroundWorker _worker;
        private PowerNetDatabaseAdapter _powerNet;
        private INodeVoltageCalculator _calculator;
        private readonly IReadOnlyConnectorData _connectorData;

        public CalculationThread(IReadOnlyConnectorData connectorData)
        {
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _connectorData = connectorData;
            _worker = new BackgroundWorker();
            _worker.DoWork += CalculatePowerNetDirect;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += CalculationFinished;
        }

        public double Progress
        {
            get { return _calculator == null ? 0 : _calculator.Progress; }
        }

        public double RelativePowerError
        {
            get { return _calculator == null ? 1 : _calculator.RelativePowerError; }
        }

        public bool IsCalculationRunning
        {
            get
            {
                lock (_isCalculationRunningMutex)
                {
                    return _isCalculationRunning;
                }
            }

            set
            {
                lock (_isCalculationRunningMutex)
                {
                    _isCalculationRunning = value;
                }
            }
        }

        public void CalculatePowerNet()
        {
            lock (_isCalculationRunningMutex)
            {
                if (_isCalculationRunning)
                {
                    Log("calculation already running");
                    return;
                }
                _isCalculationRunning = true;
            }

            try
            {
                Log("parsing the power net");
                _powerNet = new PowerNetDatabaseAdapter(_connectorData.InputFile);
            }
            catch (Exception exception)
            {
                Log("could not open selected file " + _connectorData.InputFile);
                Log(exception.Message);
                IsCalculationRunning = false;
                return;
            }

            _calculator = _connectorData.CreateCalculator();
            _worker.RunWorkerAsync();
        }

        public delegate void LogHandler(string message);

        public event LogHandler Log;

        private void CalculationFinished(object sender, RunWorkerCompletedEventArgs e)
        {
            IsCalculationRunning = false;
        }

        private void CalculatePowerNetDirect(object sender, DoWorkEventArgs e)
        {
            var stopWatch = new Stopwatch();

            try
            {
                Log("calculating the power net");
                double relativePowerError;
                stopWatch.Start();
                var success = _powerNet.CalculateNodeVoltages(_calculator, 1, out relativePowerError);
                stopWatch.Stop();
                Log("finished calculation of power net after " + stopWatch.Elapsed.TotalSeconds + "s" +
                    (success ? " with a relative power mismatch of " + relativePowerError : ", but was not able to calculate the power net"));

            }
            catch (Exception exception)
            {
                Log("was not able to calculate the power net because an exception occured:");
                Log(exception.Message);
            }
        }
    }
}
