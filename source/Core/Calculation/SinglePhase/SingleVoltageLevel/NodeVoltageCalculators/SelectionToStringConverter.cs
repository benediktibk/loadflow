using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class SelectionToStringConverter : IValueConverter
    {
        private readonly Dictionary<Selection, string> _mappingForward;
        private readonly Dictionary<string, Selection> _mappingBackward;

        public SelectionToStringConverter()
        {
            _mappingForward = new Dictionary<Selection, string>();
            _mappingBackward = new Dictionary<string, Selection>();

            var allPossibleCalculators =
                (Selection[])Enum.GetValues(typeof(Selection));

            foreach (var calculator in allPossibleCalculators)
            {
                var calculatorConverted = Convert(calculator);
                _mappingForward.Add(calculator, calculatorConverted);
                _mappingBackward.Add(calculatorConverted, calculator);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!(value is Selection))
                throw new ArgumentException("value has invalid type");

            var calculator = (Selection) value;
            return _mappingForward[calculator];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var name = value as string;

            if (name == null)
                throw new ArgumentException("value has invalid type");

            return _mappingBackward[name];
        }

        public static IEnumerable<string> AllPossibleStrings
        {
            get
            {
                var allPossibleCalculators =
                    (Selection[]) Enum.GetValues(typeof (Selection));
                return from calculator in allPossibleCalculators select Convert(calculator);
            }
        }

        private static string Convert(Selection calculator)
        {
            switch (calculator)
            {
                case Selection.NodePotential:
                    return "node potential";
                case Selection.CurrentIteration:
                    return "current iteration";
                case Selection.NewtonRaphson:
                    return "newton raphson";
                case Selection.FastDecoupledLoadFlow:
                    return "fast decoupled load flow";
                case Selection.HolomorphicEmbeddedLoadFlow:
                    return "holomorphic embedded load flow";
                case Selection.HolomorphicEmbeddedLoadFlowWithCurrentIteration:
                    return "holomorphic embedded load flow with current iteration";
                case Selection.HolomorphicEmbeddedLoadFlowWithNewtonRaphson:
                    return "holomorphic embedded load flow with newton raphson";
                default:
                    throw new ArgumentOutOfRangeException("calculator");
            }
        }
    }
}
