using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Database : DbContext, INotifyPropertyChanged
    {
        #region variables

        private PowerNet _selectedPowerNet;

        #endregion

        #region constructor

        public Database()
        {
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

        public DbSet<PowerNet> PowerNets { get; set; }

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
