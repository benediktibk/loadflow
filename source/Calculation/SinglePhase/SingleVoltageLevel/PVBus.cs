using System;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PVBus
    {
        #region variables

        private readonly int _id;
        private readonly double _realPower;
        private readonly double _voltageMagnitude;

        #endregion

        #region constructor

        public PVBus(int ID, double realPower, double voltageMagnitude)
        {
            if (ID < 0)
                throw new ArgumentOutOfRangeException("ID", "mustn't be negative");

            if (voltageMagnitude < 0)
                throw new ArgumentOutOfRangeException("voltageMagnitude", "mustn't be negative");

            _id = ID;
            _realPower = realPower;
            _voltageMagnitude = voltageMagnitude;
        }

        #endregion

        #region properties

        public int ID
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

        #endregion
    }
}
