using System;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PqBus
    {
        private readonly int _id;
        private readonly Complex _power;

        public PqBus(int id, Complex power)
        {

            if (id < 0)
                throw new ArgumentOutOfRangeException("id", "mustn't be negative");

            _id = id;
            _power = power;
        }

        public int Id
        {
            get { return _id; }
        }

        public Complex Power
        {
            get { return _power; }
        }
    }
}
