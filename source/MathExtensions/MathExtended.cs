
using System;

namespace MathExtensions
{
    public static class MathExtended
    {
        public static long Factorial(long x)
        {
            if (x < 0)
                throw new ArgumentOutOfRangeException("x", "x must be positive");

            if (x == 0)
                return 1;

            long result = 1;
            for (long i = 2; i <= x; ++i)
                result *= i;

            return result;
        }
    }
}
