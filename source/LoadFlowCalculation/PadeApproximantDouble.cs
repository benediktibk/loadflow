
namespace LoadFlowCalculation
{
    public class PadeApproximantDouble : PadeApproximant<double>
    {
        public PadeApproximantDouble(int m, int n, PowerSeries<double> powerSeries) :
            base(m, n, powerSeries, new CalculatorDouble())
        { }
    }
}
