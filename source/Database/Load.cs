using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Load : INotifyPropertyChanged
    {
        #region variables

        private string _name;
        private Node _node;
        private Complex _value;

        #endregion

        #region constructor

        public Load()
        {
            Value = new Complex();
        }

        #endregion

        #region properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;

                _name = value;
                NotifyPropertyChanged();
            }
        }

        public Node Node
        {
            get { return _node; }
            set
            {
                if (_node == value) return;

                _node = value;
                NotifyPropertyChanged();
            }
        }


        public Complex Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;

                _value = value;
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
