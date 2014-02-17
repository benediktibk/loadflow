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
        private uint _id;

        public Complex Power
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

        public Complex Voltage
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

        public uint ID
        {
            get { return _id; }
        }

        public bool VoltageIsKnown
        {
            get { return _voltageSet; }
        }

        public bool PowerIsKnown
        {
            get { return _powerSet; }
        }

        public Node(uint id)
        {
            _id = id;
            _voltageSet = false;
            _powerSet = false;
        }
    }
}
