using System.Numerics;

namespace LoadFlowCalculation
{
    public class PadeApproximantComplex : PadeApproximant<Complex>
    {
        public PadeApproximantComplex(int m, int n, PowerSeries<Complex> powerSeries) :
            base(m, n, powerSeries, new CalculatorComplex())
        { }
    }
}
