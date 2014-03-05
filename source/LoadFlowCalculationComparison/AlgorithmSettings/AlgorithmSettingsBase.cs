using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class AlgorithmSettingsBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}