using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Model : INotifyPropertyChanged
    {
        #region variables

        private PowerNet _selectedPowerNet;
        private Connection _connection;

        #endregion

        #region constructor

        public Model()
        {
            PowerNets = new ObservableCollection<PowerNet>();
            Connection = new Connection();
        }

        #endregion

        #region public functions

        public void ReadFromDatabase()
        {
            Connection.ReadPowerNets(PowerNets);
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
                if (_selectedPowerNetChanged != null)
                    _selectedPowerNetChanged();
            }
        }

        public Connection Connection
        {
            get { return _connection; }
            set
            {
                if (_connection == value) return;

                _connection = value;
                NotifyPropertyChanged();
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
