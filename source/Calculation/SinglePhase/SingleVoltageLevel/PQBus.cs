using System;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PQBus
    {
        #region variables

        private readonly int _id;
        private readonly Complex _power;

        #endregion

        #region constructor

        public PQBus(int ID, Complex power)
        {

            if (ID < 0)
                throw new ArgumentOutOfRangeException("ID", "mustn't be negative");

            _id = ID;
            _power = power;
        }

        #endregion

        #region properties

        public int ID
        {
            get { return _id; }
        }

        public Complex Power
        {
            get { return _power; }
        }

        #endregion
    }
}
