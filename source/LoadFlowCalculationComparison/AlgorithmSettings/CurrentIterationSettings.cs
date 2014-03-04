using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class CurrentIterationSettings : INotifyPropertyChanged
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

                _maximumIterations = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
