namespace MathExtensions
{
    public class PowerSeriesDecimalComplex : PowerSeries<DecimalComplex>
    {
        public PowerSeriesDecimalComplex(int numberOfCoefficients) : base(numberOfCoefficients, new CalculatorDecimalComplex())
        { }
    }
}
