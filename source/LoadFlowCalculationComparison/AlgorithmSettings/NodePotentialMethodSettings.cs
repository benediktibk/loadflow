
namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class NodePotentialMethodSettings : AlgorithmSettingsBase
    {
        private double _singularityDetection = 0.00001;

        public double SingularityDetection 
        { 
            get { return _singularityDetection; }
            set
            {
                if (value == _singularityDetection) 
                    return;

                _singularityDetection = value;
                OnPropertyChanged();
            } 
        }
    }
}
