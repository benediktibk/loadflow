using System;
using System.Numerics;

namespace LoadFlowCalculation
{
    public class Node
    {
        private System.Numerics.Complex _power;
        private bool _powerSet;
        private System.Numerics.Complex _voltage;
        private bool _voltageSet;

        public System.Numerics.Complex Power
        {
            get
            {
                if (!_powerSet)
                    throw new PowerNotSetException();
                else
                    return _power;
            }
            set
            {
                _power = value;
                _powerSet = true;
            }
        }

        public System.Numerics.Complex Voltage
        {
            get
            {
                if (!_voltageSet)
                    throw new VoltageNotSetException();
                else
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
