using System.Globalization;
using System.Windows.Controls;

namespace CalculationComparison
{
    class NotNegativeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var valueParsed = double.Parse(value.ToString());
            return valueParsed < 0 ? new ValidationResult(false, "please enter a non-negative value") : new ValidationResult(true, null);
        }
    }
}
