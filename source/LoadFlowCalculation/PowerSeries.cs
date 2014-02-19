using System;
using System.Numerics;

namespace LoadFlowCalculation
{
    public class PowerSeries
    {
        private Complex[] _coefficients;

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
    }
}
