using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Model : INotifyPropertyChanged
    {
        #region variables

        private PowerNet _selectedPowerNet;

        #endregion

        #region constructor

        public Model()
        {
            PowerNets = new ObservableCollection<PowerNet>();
        }

        #endregion

        #region properties

        [NotMapped]
        public PowerNet SelectedPowerNet 
        { 
            get { return _selectedPowerNet; }
            set
            {
                if (_selectedPowerNet == value) return;

                _selectedPowerNet = value;
                NotifyPropertyChanged();
                if (_selectedPowerNetChanged != null)
                    _selectedPowerNetChanged();
            }
        }

        public ObservableCollection<PowerNet> PowerNets { get; set; }

        #endregion

        #region events

        public delegate void PowerNetChangedEventHandler();

        private PowerNetChangedEventHandler _selectedPowerNetChanged;
        public event PowerNetChangedEventHandler SelectedPowerNetChanged
        {
            add
            {
                if (_selectedPowerNetChanged == null || !_selectedPowerNetChanged.GetInvocationList().Contains(value))
                    _selectedPowerNetChanged += value;
            }
            remove
            {
                _selectedPowerNetChanged -= value;
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
