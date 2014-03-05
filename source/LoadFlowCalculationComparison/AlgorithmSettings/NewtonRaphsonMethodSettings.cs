
namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class NewtonRaphsonMethodSettings : AlgorithmSettingsBase
    {
        private double _targetPrecision = 0.00001;
        private int _maximumIterations = 1000;

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
    }
}
