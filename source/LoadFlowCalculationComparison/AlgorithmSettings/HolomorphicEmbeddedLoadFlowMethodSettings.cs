using System;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class HolomorphicEmbeddedLoadFlowMethodSettings : ChangeAllowable
    {
        private double _targetPrecision = 0.00001;
        private int _maximumNumberOfCoefficients = 50;

        public HolomorphicEmbeddedLoadFlowMethodSettings(GeneralSettings generalSettings) : base(generalSettings)
        { }

        public double TargetPrecision
        {
            get { return _targetPrecision; }
            set
            {
                if (value == _targetPrecision)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

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

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _maximumNumberOfCoefficients = value;
                OnPropertyChanged();
            }
        }
    }
}
