using System;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PvBus
    {
        private readonly int _id;
        private readonly double _realPower;
        private readonly double _voltageMagnitude;

        public PvBus(int id, double realPower, double voltageMagnitude)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id", "mustn't be negative");

            if (voltageMagnitude < 0)
                throw new ArgumentOutOfRangeException("voltageMagnitude", "mustn't be negative");

            _id = id;
            _realPower = realPower;
            _voltageMagnitude = voltageMagnitude;
        }

        public int Id
        {
            get { return _id; }
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
