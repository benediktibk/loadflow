using System.Numerics;

namespace AnalyticContinuation
{
    public class CalculatorComplex : CalculatorGeneric<Complex>
    {
        public override Complex Add(Complex a, Complex b)
        {
            return a + b;
        }

        public override Complex Subtract(Complex a, Complex b)
        {
            return a - b;
        }

        public override Complex Multiply(Complex a, Complex b)
        {
            return a * b;
        }

        public override Complex Divide(Complex a, Complex b)
        {
            return a / b;
        }

        public override Complex AssignFromDouble(double x)
        {
            return new Complex(x, 0);
        }
    }
}
