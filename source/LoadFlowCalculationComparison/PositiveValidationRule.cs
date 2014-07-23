using System.Globalization;
using System.Windows.Controls;

namespace CalculationComparison
{
    class PositiveValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var valueParsed = double.Parse(value.ToString());
            return valueParsed <= 0 ? new ValidationResult(false, "please enter a positive value") : new ValidationResult(true, null);
        }
    }
}
