using System.Numerics;

namespace AnalyticContinuation
{
    public class CalculatorComplex : ICalculatorGeneric<Complex>
    {
        public Complex Add(Complex a, Complex b)
        {
            return a + b;
        }

        public Complex Subtract(Complex a, Complex b)
        {
            return a - b;
        }

        public Complex Multiply(Complex a, Complex b)
        {
            return a * b;
        }

        public Complex Divide(Complex a, Complex b)
        {
            return a / b;
        }

        public Complex AssignFromDouble(double x)
        {
            return new Complex(x, 0);
        }
    }
}
