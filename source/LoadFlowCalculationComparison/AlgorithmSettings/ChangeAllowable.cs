using System.ComponentModel;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    class ChangeAllowable : NotifyPropertyChanged
    {
        private readonly GeneralSettings _generalSettings;

        public ChangeAllowable(GeneralSettings generalSettings)
        {
            generalSettings.PropertyChanged += GeneralSettingsPropertyChanged;
            _generalSettings = generalSettings;
        }

        void GeneralSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ChangesAllowed")
                OnPropertyChanged(e.PropertyName);
        }

        public bool ChangesAllowed
        {
            get { return _generalSettings.ChangesAllowed; }
        }
    }
}
