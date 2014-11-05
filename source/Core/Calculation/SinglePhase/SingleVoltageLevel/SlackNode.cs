using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class SlackNode : INode
    {
        public SlackNode(Complex voltage)
        {
            Voltage = voltage;
        }

        public Complex Voltage { get; private set; }

        public void AddTo(IList<int> indexOfSlackBuses, IList<int> indexOfPqBuses, IList<int> indexOfPvBuses, int index)
        {
            indexOfSlackBuses.Add(index);
        }

        public void AddTo(IList<PvBus> pvBuses, int index)
        {
        }

        public void AddTo(IList<PqBus> pqBuses, int index)
        {
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
            voltages[index] = Voltage;
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
        }
    }
}
