using System;
using System.Globalization;
using System.Windows.Data;

namespace Database
{
    public class NodeToNodeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string Convert(Node node)
        {
            return node.Name + " [" + node.Id + "]";
        }
    }
}
