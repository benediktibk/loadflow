using System;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PvNodeWithIndex
    {
        private readonly int _index;
        private readonly double _realPower;
        private readonly double _voltageMagnitude;

        public PvNodeWithIndex(int index, double realPower, double voltageMagnitude)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "mustn't be negative");

            if (voltageMagnitude < 0)
                throw new ArgumentOutOfRangeException("voltageMagnitude", "mustn't be negative");

            _index = index;
            _realPower = realPower;
            _voltageMagnitude = voltageMagnitude;
        }

        public int Index
        {
            get { return _index; }
        }

        public double RealPower
        {
            get { return _realPower; }
        }

        public double VoltageMagnitude
        {
            get { return _voltageMagnitude; }
        }
    }
}
