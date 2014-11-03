using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Database;

namespace DatabaseUI
{
    public class NodeVoltageCalculatorSelectionToStringConverter : IValueConverter
    {
        private readonly Dictionary<NodeVoltageCalculatorSelection, string> _mappingForward;
        private readonly Dictionary<string, NodeVoltageCalculatorSelection> _mappingBackward;

        public NodeVoltageCalculatorSelectionToStringConverter()
        {
            _mappingForward = new Dictionary<NodeVoltageCalculatorSelection, string>();
            _mappingBackward = new Dictionary<string, NodeVoltageCalculatorSelection>();

            var allPossibleCalculators =
                (NodeVoltageCalculatorSelection[])Enum.GetValues(typeof(NodeVoltageCalculatorSelection));

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

            if (!(value is NodeVoltageCalculatorSelection))
                throw new ArgumentException("value has invalid type");

            var calculator = (NodeVoltageCalculatorSelection) value;
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
                    (NodeVoltageCalculatorSelection[]) Enum.GetValues(typeof (NodeVoltageCalculatorSelection));
                return from calculator in allPossibleCalculators select Convert(calculator);
            }
        }

        private static string Convert(NodeVoltageCalculatorSelection calculator)
        {
            switch (calculator)
            {
                case NodeVoltageCalculatorSelection.NodePotential:
                    return "node potential";
                case NodeVoltageCalculatorSelection.CurrentIteration:
                    return "current iteration";
                case NodeVoltageCalculatorSelection.NewtonRaphson:
                    return "newton raphson";
                case NodeVoltageCalculatorSelection.FastDecoupledLoadFlow:
                    return "fast decoupled load flow";
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlow:
                    return "holomorphic embedded load flow";
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowHighPrecision:
                    return "holomorphic embedded load flow, high precision";
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowWithCurrentIteration:
                    return "holomorphic embedded load flow with current iteration";
                case NodeVoltageCalculatorSelection.HolomorphicEmbeddedLoadFlowWithNewtonRaphson:
                    return "holomorphic embedded load flow with newton raphson";
                default:
                    throw new ArgumentOutOfRangeException("calculator");
            }
        }
    }
}
