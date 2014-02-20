using System.Numerics;

namespace LoadFlowCalculation
{
    class PowerSeriesComplex : PowerSeries<Complex>
    {
        public PowerSeriesComplex(int numberOfCoefficients) :
            base(numberOfCoefficients, new CalculatorComplex())
        { }
    }
}
