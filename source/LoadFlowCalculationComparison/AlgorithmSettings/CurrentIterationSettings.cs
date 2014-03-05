using System;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class CurrentIterationSettings : NotifyPropertyChanged
    {
        private double _terminationCriteria = 0.00001;
        private int _maximumIterations = 1000;
        
        public double TerminationCriteria
        {
            get { return _terminationCriteria; }
            set
            {
                if (value == _terminationCriteria)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _terminationCriteria = value;
                OnPropertyChanged();
            }
        }

        public int MaximumIterations
        {
            get { return _maximumIterations; }
            set
            {
                if (value == _maximumIterations)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _maximumIterations = value;
                OnPropertyChanged();
            }
        }
    }
}
