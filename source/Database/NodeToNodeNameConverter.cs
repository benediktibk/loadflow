using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Database
{
    public class NodeToNodeNameConverter : IValueConverter
    {
        private readonly Dictionary<string, Node> _mapping;

        public NodeToNodeNameConverter()
        {
            _mapping = new Dictionary<string, Node>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var node = value as Node;

            if (node == null)
                throw new ArgumentException("values has invalid type");

            return Convert(node);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var name = value as string;

            if (name == null)
                throw new ArgumentException("values has invalid type");

            return _mapping[name];
        }

        public void UpdateMapping(IEnumerable<Node> nodes)
        {
            _mapping.Clear();

            foreach (var node in nodes)
                _mapping.Add(Convert(node), node);
        }

        public static string Convert(Node node)
        {
            return node.Name + " [" + node.Id + "]";
        }
    }
}
