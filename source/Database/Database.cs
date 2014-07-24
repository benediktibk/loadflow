using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Database : INotifyPropertyChanged
    {
        #region variables

        private PowerNet _selectedPowerNet;
        private ObservableCollection<PowerNet> _powerNets;

        #endregion

        #region constructor

        public Database()
        {
            _powerNets = new ObservableCollection<PowerNet>();
        }

        #endregion

        #region properties

        public PowerNet SelectedPowerNet 
        { 
            get { return _selectedPowerNet; }
            set
            {
                if (_selectedPowerNet == value) return;

                _selectedPowerNet = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<PowerNet> PowerNets
        {
            get { return _powerNets; }
            set
            {
                if (_powerNets == value) return;

                _powerNets = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
