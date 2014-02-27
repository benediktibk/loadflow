using System;

namespace MathExtensions
{
    public interface ICalculatorGeneric<T> where T : struct, IEquatable<T>, IFormattable
    {
        T Add(T a, T b);
        T Subtract(T a, T b);
        T Multiply(T a, T b);
        T Divide(T a, T b);
        T Pow(T a, int exponent);
        T AssignFromDouble(double x);
        bool IsValidNumber(T x);
    }
}
