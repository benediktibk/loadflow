using System;
using System.Globalization;
using System.Windows.Data;

namespace DatabaseUI
{
    public class ConnectedToButtonLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                throw new ArgumentException("value has invalid type");

            var connected = (bool) value;
            return connected ? "disconnect" : "connect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
