
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

        public static long BinomialCoefficient(long n, long k)
        {
            if (k < 0)
                throw new ArgumentOutOfRangeException("k", "must be positive");

            if (k > n)
                throw new ArgumentOutOfRangeException("k", "must not be greater than n");

            if (n == k || k == 0)
                return 1;

            var nominator = n;

            for (var i = n - 1; i > n - k; --i)
                nominator *= i;

            var denominator = Factorial(k);
            return nominator/denominator;
        }
    }
}
