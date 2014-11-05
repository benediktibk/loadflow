using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class Node : IReadOnlyNode
    {
        private Complex _power;
        private Complex _voltage;
        private double _voltageMagnitude;

        public Node()
        {
            VoltageIsKnown = false;
            PowerIsKnown = false;
            RealPowerIsKnown = false;
            VoltageMagnitudeIsKnown = false;
        }

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

        public void AddTo(IList<int> indexOfSlackBuses, IList<int> indexOfPqBuses, IList<int> indexOfPvBuses, int index)
        {
            if (IsSlackBus)
                indexOfSlackBuses.Add(index);
            else if (IsPQBus)
                indexOfPqBuses.Add(index);
            else if (IsPVBus)
                indexOfPvBuses.Add(index);
            else
                throw new InvalidOperationException("invalid bus type (neither PV, PQ or slack bus)");
        }

        public void SetValueIn(Vector<Complex> knownVoltages, int index)
        {
            knownVoltages[index] = Voltage;
        }

        public void AddTo(IList<PvBus> pvBuses, int index)
        {
            pvBuses.Add(new PvBus(index, RealPower, VoltageMagnitude));
        }

        public void AddTo(IList<PqBus> pqBuses, int index)
        {
            pqBuses.Add(new PqBus(index, Power));
        }
    }
}
