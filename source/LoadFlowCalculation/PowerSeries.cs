using System;
using System.Numerics;

namespace LoadFlowCalculation
{
    public class PowerSeries
    {
        private readonly Complex[] _coefficients;

        public PowerSeries(int numberOfCoefficients)
        {
            if (numberOfCoefficients <= 1)
                throw new ArgumentOutOfRangeException();

            _coefficients = new Complex[numberOfCoefficients];

            for (var i = 0; i < _coefficients.Length; ++i)
                _coefficients[i] = new Complex(0, 0);
        }

        public void SetCoefficient(int index, Complex value)
        {
            _coefficients[index] = value;
        }

        public Complex Evaluate(Complex x)
        {
            var result = new Complex(0, 0);

            for (var i = _coefficients.Length - 1; i >= 0; --i)
                result = result*x + _coefficients[i];

            return result;
        }

        public Complex EvaluateAt1()
        {
            var result = new Complex(0, 0);

            foreach (var coefficient in _coefficients)
                result = result + coefficient;

            return result;
        }

        public static PowerSeries CreateExponential(int numberOfCoefficients)
        {
            var function = new PowerSeries(numberOfCoefficients);
            function.SetCoefficient(0, 1);
            var divisor = 1;

            for (var i = 1; i < numberOfCoefficients; ++i)
            {
                function.SetCoefficient(i, 1.0 / divisor);
                divisor *= (i + 1);
            }

            return function;
        }

        public static PowerSeries CreateSin(int numberOfCoefficients)
        {
            var function = new PowerSeries(numberOfCoefficients);

            for (var i = 0; i < numberOfCoefficients; ++i)
            {
                if (i%2 == 0)
                    function.SetCoefficient(i, 0);
                else
                {
                    var n = (i - 1)/2;
                    var sign = 0.0;

                    if (n%2 == 0)
                        sign = 1;
                    else
                        sign = -1;

                    function.SetCoefficient(i, sign/Factorial(i));
                }
            }

            return function;
        }

        public static int Factorial(int x)
        {
            if (x == 0)
                return 1;

            var result = 1;
            for (var i = 2; i <= x; ++i)
                result *= i;

            return result;
        }
    }
}
