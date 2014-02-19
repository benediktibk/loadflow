using System;
using System.Numerics;

namespace LoadFlowCalculation
{
    public class PadeApproximant
    {
        private PowerSeries _p;
        private PowerSeries _q;

        public PadeApproximant(int m, int n, PowerSeries powerSeries)
        {
            if (m < 1 || n < 1)
                throw new ArgumentOutOfRangeException();

            _p = new PowerSeries(m);
            _q = new PowerSeries(n);
        }

        public Complex Evaluate(Complex x)
        {
            return _p.Evaluate(x) / _q.Evaluate(x);
        }

        public Complex EvaluateAt1()
        {
            return _p.EvaluateAt1() / _q.EvaluateAt1();
        }
    }
}
