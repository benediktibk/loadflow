﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Database
{
    public class Node : INotifyPropertyChanged
    {
        #region variables

        private long _id;
        private string _name;
        private double _nominalVoltage;
        private double _voltageReal;
        private double _voltageImaginary;

        #endregion

        #region constructor

        public Node()
        {
            NominalVoltage = 1;
            VoltageReal = 0;
            VoltageImaginary = 0;
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

        public long Id
        {
            get { return _id; }
            set
            {
                if (_id == value) return;

                _id = value;
                NotifyPropertyChanged();
            }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
            set
            {
                if (_nominalVoltage == value) return;

                _nominalVoltage = value;
                NotifyPropertyChanged();
            }
        }

        public double VoltageReal
        {
            get { return _voltageReal; }
            set
            {
                if (_voltageReal == value) return;

                _voltageReal = value;
                NotifyPropertyChanged();
            }
        }

        public double VoltageImaginary
        {
            get { return _voltageImaginary; }
            set
            {
                if (_voltageImaginary == value) return;

                _voltageImaginary = value;
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