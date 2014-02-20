namespace LoadFlowCalculation
{
    public class PowerSeriesDouble : PowerSeries<double>
    {
        public PowerSeriesDouble(int numberOfCoefficients) :
            base(numberOfCoefficients, new CalculatorDouble())
        { }
    }
}
