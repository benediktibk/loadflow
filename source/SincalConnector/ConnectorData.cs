using System.ComponentModel;

namespace SincalConnector
{
    public class ConnectorData : INotifyPropertyChanged
    {
        #region variables

        private string _inputFile;
        private string _loggingOutput;
        private int _sizeOfDataType;
        private int _countOfCoefficients;

        #endregion
        #region constructor

        public ConnectorData()
        {
            SizeOfDataType = 64;
            CountOfCoefficients = 50;
        }

        #endregion

        #region properties

        public int SizeOfDataType
        {
            get { return _sizeOfDataType; }
            set
            {
                if (value == _sizeOfDataType) return;

                _sizeOfDataType = value;
                OnPropertyChanged("SizeOfDataType");
                OnPropertyChanged("IsValid");
            }
        }

        public int CountOfCoefficients
        {
            get { return _countOfCoefficients; }
            set
            {
                if (value == _countOfCoefficients) return;

                _countOfCoefficients = value;
                OnPropertyChanged("CountOfCoefficients");
                OnPropertyChanged("IsValid");
            }
        }

        public string InputFile
        {
            get { return _inputFile; }
            set
            {
                if (value == _inputFile) return;

                _inputFile = value;
                OnPropertyChanged("InputFile");
            }
        }

        public string LoggingOutput
        {
            get { return _loggingOutput; }
            set
            {
                if (value == _loggingOutput) return;

                _loggingOutput = value;
                OnPropertyChanged("LoggingOutput");
            }
        }

        public bool IsValid
        {
            get { return SizeOfDataType > 0 && CountOfCoefficients > 2; }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
