namespace AnalyticContinuation
{
    public abstract class CalculatorGeneric<T>
    {
        public abstract T Add(T a, T b);
        public abstract T Subtract(T a, T b);
        public abstract T Multiply(T a, T b);
        public abstract T Divide(T a, T b);
        public abstract T AssignFromDouble(double x);
    }
}
