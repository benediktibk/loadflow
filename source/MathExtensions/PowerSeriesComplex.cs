using System.Numerics;

namespace MathExtensions
{
    public class PowerSeriesComplex : PowerSeries<Complex>
    {
        public PowerSeriesComplex(int numberOfCoefficients) : base(numberOfCoefficients, new CalculatorComplex())
        { }
    }
}
