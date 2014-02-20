namespace LoadFlowCalculation
{
    public class CalculatorDouble : CalculatorGeneric<double>
    {
        public override double Add(double a, double b)
        {
            return a + b;
        }

        public override double Subtract(double a, double b)
        {
            return a - b;
        }

        public override double Multiply(double a, double b)
        {
            return a * b;
        }

        public override double Divide(double a, double b)
        {
            return a / b;
        }

        public override double AssignFromDouble(double x)
        {
            return x;
        }
    }
}
