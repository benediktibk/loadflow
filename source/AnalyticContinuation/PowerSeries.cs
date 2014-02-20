using System;

namespace AnalyticContinuation
{
    public class PowerSeries<T> where T : new()
    {
        private readonly T[] _coefficients;
        private readonly CalculatorGeneric<T> _calculator; 

        public PowerSeries(int numberOfCoefficients, CalculatorGeneric<T> calculator)
        {
            _calculator = calculator;

            if (numberOfCoefficients <= 1)
                throw new ArgumentOutOfRangeException();

            _coefficients = new T[numberOfCoefficients];

            for (var i = 0; i < _coefficients.Length; ++i)
                _coefficients[i] = new T();
        }

        public void SetCoefficient(int index, T value)
        {
            _coefficients[index] = value;
        }

        public T Evaluate(T x)
        {
            var result = new T();

            for (var i = _coefficients.Length - 1; i >= 0; --i)
                result = _calculator.Add(_calculator.Multiply(result, x), _coefficients[i]);

            return result;
        }

        public T EvaluateAt1()
        {
            var result = new T();

            foreach (var coefficient in _coefficients)
                result = _calculator.Add(result, coefficient);

            return result;
        }

        public CalculatorGeneric<T> getCalculator()
        {
            return _calculator;
        }

        public static PowerSeries<T> CreateExponential(int numberOfCoefficients, CalculatorGeneric<T> calculator)
        {
            var function = new PowerSeries<T>(numberOfCoefficients, calculator);
            function.SetCoefficient(0, calculator.AssignFromDouble(1));
            var divisor = 1;

            for (var i = 1; i < numberOfCoefficients; ++i)
            {
                function.SetCoefficient(i, calculator.AssignFromDouble(1.0/divisor));
                divisor *= (i + 1);
            }

            return function;
        }

        public static PowerSeries<T> CreateSin(int numberOfCoefficients, CalculatorGeneric<T> calculator)
        {
            var function = new PowerSeries<T>(numberOfCoefficients, calculator);

            for (var i = 0; i < numberOfCoefficients; ++i)
            {
                if (i % 2 == 0)
                    function.SetCoefficient(i, new T());
                else
                {
                    var n = (i - 1) / 2;
                    double sign;

                    if (n % 2 == 0)
                        sign = 1;
                    else
                        sign = -1;

                    function.SetCoefficient(i, calculator.AssignFromDouble(sign/Factorial(i)));
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
