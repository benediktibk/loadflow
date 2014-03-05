using System;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class NodePotentialMethodSettings : ChangeAllowable
    {
        private double _singularityDetection = 0.00001;

        public NodePotentialMethodSettings(GeneralSettings generalSettings) : base(generalSettings)
        { }

        public double SingularityDetection 
        { 
            get { return _singularityDetection; }
            set
            {
                if (value == _singularityDetection)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _singularityDetection = value;
                OnPropertyChanged();
            } 
        }
    }
}
