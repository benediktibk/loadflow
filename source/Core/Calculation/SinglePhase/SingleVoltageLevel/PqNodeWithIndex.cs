using System;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PqNodeWithIndex
    {
        private readonly int _index;
        private readonly Complex _power;

        public PqNodeWithIndex(int index, Complex power)
        {

            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "mustn't be negative");

            _index = index;
            _power = power;
        }

        public int Index
        {
            get { return _index; }
        }

        public Complex Power
        {
            get { return _power; }
        }
    }
}
