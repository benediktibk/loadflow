using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Database.Annotations;

namespace Database
{
    public class Node : INotifyPropertyChanged
    {
        public Node()
        {
            NominalVoltage = 1;
            Voltage = new Complex();
        }

        public long Id { get; set; }
        public double NominalVoltage { get; set; }
        public string Name { get; set; }
        public Complex Voltage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
