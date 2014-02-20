using System.Numerics;

namespace AnalyticContinuation
{
    public class PowerSeriesComplex : PowerSeries<Complex>
    {
        public PowerSeriesComplex(int numberOfCoefficients) :
            base(numberOfCoefficients, new CalculatorComplex())
        { }
    }
}
