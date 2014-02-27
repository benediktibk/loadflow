
namespace MathExtensions
{
    public class MathExtended
    {
        public static ulong Factorial(ulong x)
        {
            if (x == 0)
                return 1;

            ulong result = 1;
            for (ulong i = 2; i <= x; ++i)
                result *= i;

            return result;
        }
    }
}
