using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Data : INotifyPropertyChanged
    {
        #region variables

        private PowerNet _selectedPowerNet;

        #endregion

        #region constructor

        public Data()
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
            }
        }

        public ObservableCollection<PowerNet> PowerNets { get; set; }

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
