namespace AnalyticContinuation
{
    public interface ICalculatorGeneric<T>
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
        T AssignFromDouble(double x);
    }
}
