using System;
using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class Node
    {
        private Complex _power;
        private bool _powerSet;
        private Complex _voltage;
        private bool _voltageSet;
        private double _voltageMagnitude;
        private bool _voltageMagnitudeSet;
        private bool _realPowerSet;

        public Complex Power
        {
            get
            {
                if (!_powerSet)
                    throw new ArgumentOutOfRangeException();
                
                return _power;
            }
            set
            {
                _power = value;
                _powerSet = true;
                _realPowerSet = true;
            }
        }

        public Complex Voltage
        {
            get
            {
                if (!_voltageSet)
                    throw new ArgumentOutOfRangeException();
                
                return _voltage;
            }
            set
            {
                _voltage = value;
                _voltageMagnitude = value.Magnitude;
                _voltageSet = true;
                _voltageMagnitudeSet = true;
            }
        }

        public double RealPower
        {
            get
            {
                if (!_realPowerSet)
                    throw new ArgumentOutOfRangeException();

                return _power.Real;
            }
            set
            {
                _power = new Complex(value, 0);
                _realPowerSet = true;
                _powerSet = false;
            }
        }

        public double VoltageMagnitude
        {
            get
            {
                if (!_voltageMagnitudeSet)
                    throw new ArgumentOutOfRangeException();

                return _voltageMagnitude;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "mustn't be negative");

                _voltageMagnitude = value;
                _voltageMagnitudeSet = true;
                _voltageSet = false;
            }
        }

        public bool VoltageIsKnown
        {
            get { return _voltageSet; }
        }

        public bool PowerIsKnown
        {
            get { return _powerSet; }
        }

        public bool VoltageMagnitudeIsKnown
        {
            get { return _voltageMagnitudeSet; }
        }

        public bool RealPowerIsKnown
        {
            get { return _realPowerSet; }
        }

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

        public Node()
        {
            _voltageSet = false;
            _powerSet = false;
            _realPowerSet = false;
            _voltageMagnitudeSet = false;
        }
    }
}
