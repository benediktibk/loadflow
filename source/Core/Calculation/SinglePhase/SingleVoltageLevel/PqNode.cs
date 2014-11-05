using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PqNode : INode
    {
        public PqNode(Complex power)
        {
            Power = power;
        }

        public Complex Power { get; private set; }

        public void AddTo(IList<int> indexOfSlackBuses, IList<int> indexOfPqBuses, IList<int> indexOfPvBuses, int index)
        {
            indexOfPqBuses.Add(index);
        }

        public void AddTo(IList<PvBus> pvBuses, int index)
        {
        }

        public void AddTo(IList<PqBus> pqBuses, int index)
        {
            pqBuses.Add(new PqBus(index, Power));
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
            powers[index] = Power;
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
        }
    }
}
