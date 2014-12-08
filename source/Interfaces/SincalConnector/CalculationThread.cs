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
        private readonly BackgroundWorker _workerDirectCalculation;
        private readonly BackgroundWorker _workerConvergenceBorderSearch;
        private PowerNetDatabaseAdapter _powerNet;
        private INodeVoltageCalculator _calculator;
        private readonly IReadOnlyConnectorData _connectorData;

        public CalculationThread(IReadOnlyConnectorData connectorData)
        {
            _isCalculationRunning = false;
            _isCalculationRunningMutex = new Mutex();
            _connectorData = connectorData;
            _workerDirectCalculation = new BackgroundWorker();
            _workerConvergenceBorderSearch = new BackgroundWorker();
            _workerDirectCalculation.DoWork += CalculatePowerNetDirect;
            _workerConvergenceBorderSearch.DoWork += SearchForConvergenceBorder;
            _workerDirectCalculation.RunWorkerCompleted += CalculationFinished;
            _workerConvergenceBorderSearch.RunWorkerCompleted += CalculationFinished;
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

            if (_connectorData.ConvergenceBorderSearch)
                _workerConvergenceBorderSearch.RunWorkerAsync();
            else
                _workerDirectCalculation.RunWorkerAsync();
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

        private void SearchForConvergenceBorder(object sender, DoWorkEventArgs e)
        {
            double lower = 0;
            double upper = 1;

            try
            {
                Log("searching for the convergence border of the power net");

                while (upper - lower > 1e-4)
                {
                    var middle = (upper + lower)/2;
                    double relativePowerError;
                    var success = _powerNet.CalculateNodeVoltages(_calculator, middle, out relativePowerError);

                    if (success)
                        lower = middle;
                    else
                        upper = middle;

                    Log("convergence border lies within " + lower + " and " + upper);
                }

            }
            catch (Exception exception)
            {
                Log("was not able to calculate the power net because an exception occured:");
                Log(exception.Message);
            }
        }
    }
}
