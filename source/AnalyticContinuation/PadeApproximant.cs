using System;

namespace AnalyticContinuation
{
    public class PadeApproximant<T> where T : new()
    {
        private PowerSeries<T> _p;
        private PowerSeries<T> _q;
        private readonly CalculatorGeneric<T> _calculator; 

        public PadeApproximant(int m, int n, PowerSeries<T> powerSeries)
        {
            _calculator = powerSeries.getCalculator();
            if (m < 1 || n < 1)
                throw new ArgumentOutOfRangeException();

            _p = new PowerSeries<T>(m, _calculator);
            _q = new PowerSeries<T>(n, _calculator);
        }

        public T Evaluate(T x)
        {
            return _calculator.Divide(_p.Evaluate(x), _q.Evaluate(x));
        }

        public T EvaluateAt1()
        {
            return _calculator.Divide(_p.EvaluateAt1(), _q.EvaluateAt1());
        }
    }
}
