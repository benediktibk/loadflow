using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Model : INotifyPropertyChanged
    {
        private PowerNet _selectedPowerNet;
        private Connection _connection;
        private bool _reactToChanges;

        public Model()
        {
            PowerNets = new ObservableCollection<PowerNet>();
            Connection = new Connection();
            PowerNets.CollectionChanged += UpdateDatabaseWithChangedPowerNets;
            _reactToChanges = true;
        }

        public void ReadFromDatabase()
        {
            _reactToChanges = false;
            Connection.ReadPowerNets(PowerNets);

            foreach (var powerNet in PowerNets)
                powerNet.PropertyChanged += UpdatePowerNetInDatabase;
            _reactToChanges = true;
        }

        public void Clear()
        {
            _reactToChanges = false;
            PowerNets.Clear();
            SelectedPowerNet = null;
            _reactToChanges = true;
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void UpdateDatabaseWithChangedPowerNets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_reactToChanges)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newPowerNets = e.NewItems.Cast<PowerNet>();
                    foreach (var powerNet in newPowerNets)
                    {
                        Connection.Add(powerNet);
                        powerNet.PropertyChanged += UpdatePowerNetInDatabase;
                        powerNet.Connection = Connection;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldPowerNets = e.OldItems.Cast<PowerNet>();
                    foreach (var powerNet in oldPowerNets)
                        Connection.Remove(powerNet);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePowerNetInDatabase(object sender, PropertyChangedEventArgs e)
        {
            var powerNet = sender as PowerNet;

            if (powerNet == null)
                throw new ArgumentNullException("sender");

            Connection.Update(powerNet);
        }
    }
}
