using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class NodePotentialMethodSettings : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
