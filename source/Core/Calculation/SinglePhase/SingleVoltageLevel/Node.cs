﻿using System;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class Node : IReadOnlyNode
    {
        #region variables

        private Complex _power;
        private Complex _voltage;
        private double _voltageMagnitude;

        #endregion

        #region constructor

        public Node()
        {
            VoltageIsKnown = false;
            PowerIsKnown = false;
            RealPowerIsKnown = false;
            VoltageMagnitudeIsKnown = false;
            NominalPhaseShift = 0;
        }

        #endregion

        #region properties

        public Complex Power
        {
            get
            {
                if (!PowerIsKnown)
                    throw new ArgumentOutOfRangeException();
                
                return _power;
            }
            set
            {
                _power = value;
                PowerIsKnown = true;
                RealPowerIsKnown = true;
            }
        }

        public Complex Voltage
        {
            get
            {
                if (!VoltageIsKnown)
                    throw new ArgumentOutOfRangeException();
                
                return _voltage;
            }
            set
            {
                _voltage = value;
                _voltageMagnitude = value.Magnitude;
                VoltageIsKnown = true;
                VoltageMagnitudeIsKnown = true;
            }
        }

        public double RealPower
        {
            get
            {
                if (!RealPowerIsKnown)
                    throw new ArgumentOutOfRangeException();

                return _power.Real;
            }
            set
            {
                _power = new Complex(value, 0);
                RealPowerIsKnown = true;
                PowerIsKnown = false;
            }
        }

        public double VoltageMagnitude
        {
            get
            {
                if (!VoltageMagnitudeIsKnown)
                    throw new ArgumentOutOfRangeException();

                return _voltageMagnitude;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _voltageMagnitude = value;
                VoltageMagnitudeIsKnown = true;
                VoltageIsKnown = false;
            }
        }

        public bool VoltageIsKnown { get; private set; }

        public bool PowerIsKnown { get; private set; }

        public bool VoltageMagnitudeIsKnown { get; private set; }

        public bool RealPowerIsKnown { get; private set; }

        public bool IsPQBus
        {
            get { return PowerIsKnown && !VoltageIsKnown && !VoltageMagnitudeIsKnown; }
        }

        public bool IsPVBus
        {
            get { return VoltageMagnitudeIsKnown && RealPowerIsKnown && !VoltageIsKnown && !PowerIsKnown; }
        }

        public bool IsSlackBus
        {
            get { return VoltageIsKnown && !PowerIsKnown && !RealPowerIsKnown; }
        }

        public double NominalPhaseShift { get; set; }

        #endregion
    }
}