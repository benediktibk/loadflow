using System;
using System.Numerics;

namespace LoadFlowCalculation
{
    public class Node
    {
        private Complex _power;
        private bool _powerSet;
        private Complex _voltage;
        private bool _voltageSet;

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
                _voltageSet = true;
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

        public Node()
        {
            _voltageSet = false;
            _powerSet = false;
        }
    }
}
