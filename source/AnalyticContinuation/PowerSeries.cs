using System;
using System.Linq;

namespace AnalyticContinuation
{
    public class PowerSeries<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly T[] _coefficients;
        private readonly ICalculatorGeneric<T> _calculator; 

        public PowerSeries(int numberOfCoefficients, ICalculatorGeneric<T> calculator)
        {
            _calculator = calculator;

            if (numberOfCoefficients <= 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "there must be at least one coefficient");

            _coefficients = new T[numberOfCoefficients];

            for (var i = 0; i < _coefficients.Length; ++i)
                _coefficients[i] = new T();
        }

        public T this[int i]
        {
            get 
            {
                return i < _coefficients.Count() ? _coefficients[i] : _calculator.AssignFromDouble(0);
            }
            set
            {
                if (i >= _coefficients.Count())
                    throw new ArgumentOutOfRangeException("i", "this coefficient for the power series cant be set");

                _coefficients[i] = value;
            }
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

        public ICalculatorGeneric<T> GetCalculator()
        {
            return _calculator;
        }

        public int GetNumberOfCoefficients()
        {
            return _coefficients.Count();
        }

        public int GetDegree()
        {
            return _coefficients.Count() - 1;
        }

        public static PowerSeries<T> CreateExponential(int numberOfCoefficients, ICalculatorGeneric<T> calculator)
        {
            var function = new PowerSeries<T>(numberOfCoefficients, calculator);
            function[0] = calculator.AssignFromDouble(1);
            var divisor = 1;

            for (var i = 1; i < numberOfCoefficients; ++i)
            {
                function[i] = calculator.AssignFromDouble(1.0/divisor);
                divisor *= (i + 1);
            }

            return function;
        }

        public static PowerSeries<T> CreateSine(int numberOfCoefficients, ICalculatorGeneric<T> calculator)
        {
            var function = new PowerSeries<T>(numberOfCoefficients, calculator);

            for (var i = 0; i < numberOfCoefficients; ++i)
            {
                if (i%2 == 0)
                    function[i] = calculator.AssignFromDouble(0);
                else
                {
                    var n = (i - 1)/2;
                    double sign;

                    if (n%2 == 0)
                        sign = 1;
                    else
                        sign = -1;

                    function[i] = calculator.AssignFromDouble(sign/Factorial(i));
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
