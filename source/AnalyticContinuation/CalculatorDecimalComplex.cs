using System;

namespace AnalyticContinuation
{
    public class CalculatorDecimalComplex : ICalculatorGeneric<DecimalComplex>
    {
        public DecimalComplex Add(DecimalComplex a, DecimalComplex b)
        {
            return new DecimalComplex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        public DecimalComplex Subtract(DecimalComplex a, DecimalComplex b)
        {
            return new DecimalComplex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        public DecimalComplex Multiply(DecimalComplex a, DecimalComplex b)
        {
            return new DecimalComplex(a.Real*b.Real - a.Imaginary*b.Imaginary, a.Real*b.Imaginary + a.Imaginary*b.Real);
        }

        public DecimalComplex Divide(DecimalComplex a, DecimalComplex b)
        {
            var real = a.Real*b.Real + a.Imaginary*b.Imaginary;
            var imaginary = a.Imaginary*b.Real - a.Real*b.Imaginary;
            var divisor = b.Real*b.Real + b.Imaginary*b.Imaginary;
            return new DecimalComplex(real/divisor, imaginary/divisor);
        }

        public DecimalComplex Pow(DecimalComplex a, int exponent)
        {
            if (exponent == 0)
                return new DecimalComplex(1, 0);

            var result = a;

            for (var i = 2; i <= Math.Abs(exponent); ++i)
                result = Multiply(result, a);

            if (exponent < 0)
                result = Divide(new DecimalComplex(1, 0), result);

            return result;
        }

        public DecimalComplex AssignFromDouble(double x)
        {
            return new DecimalComplex(new decimal(x), 0);
        }

        public bool IsValidNumber(DecimalComplex x)
        {
            return true;
        }
    }
}
