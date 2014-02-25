using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticContinuation
{
    public class PowerSeriesDecimalComplex : PowerSeries<DecimalComplex>
    {
        public PowerSeriesDecimalComplex(int numberOfCoefficients) : base(numberOfCoefficients, new CalculatorDecimalComplex())
        { }
    }
}
