
namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class HolomorphicEmbeddedLoadFlowMethodSettings : NotifyPropertyChanged
    {
        private double _targetPrecision = 0.00001;
        private int _maximumNumberOfCoefficients = 50;

        public double TargetPrecision
        {
            get { return _targetPrecision; }
            set
            {
                if (value == _targetPrecision)
                    return;

                _targetPrecision = value;
                OnPropertyChanged();
            }
        }

        public int MaximumNumberOfCoefficients
        {
            get { return _maximumNumberOfCoefficients; }
            set
            {
                if (value == _maximumNumberOfCoefficients)
                    return;

                _maximumNumberOfCoefficients = value;
                OnPropertyChanged();
            }
        }
    }
}
