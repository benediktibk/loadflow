﻿using System;

namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel
{
    public class PVBus
    {
        private readonly int _id;
        private readonly double _realPower;
        private readonly double _voltageMagnitude;

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
    }
}