using System;

namespace AnalyticContinuation
{
    public class AitkenProcess<T> : IAnalyticContinuation<T> where T : struct, IEquatable<T>, IFormattable
    {
        private readonly PowerSeries<T> _powerSeries;
        private readonly ICalculatorGeneric<T> _calculator;

        public AitkenProcess(PowerSeries<T> powerSeries)
        {
            _powerSeries = powerSeries;
            _calculator = powerSeries.Calculator;
        }

        public T Evaluate(T x)
        {
            throw new NotImplementedException();
        }

        public T EvaluateAt1()
        {
            throw new NotImplementedException();
        }
    }
}
