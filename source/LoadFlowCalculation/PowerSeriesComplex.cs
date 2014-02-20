using System.Numerics;

namespace LoadFlowCalculation
{
    public class PowerSeriesComplex : PowerSeries<Complex>
    {
        public PowerSeriesComplex(int numberOfCoefficients) :
            base(numberOfCoefficients, new CalculatorComplex())
        { }
    }
}
