using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PvNode : INode
    {
        public PvNode(double realPower, double voltageMagnitude)
        {
            RealPower = realPower;
            VoltageMagnitude = voltageMagnitude;
        }

        public double RealPower { get; private set; }

        public double VoltageMagnitude { get; private set; }

        public void AddTo(IList<int> indexOfSlackBuses, IList<int> indexOfPqBuses, IList<int> indexOfPvBuses, int index)
        {
            indexOfPvBuses.Add(index);
        }

        public void AddTo(IList<PvBus> pvBuses, int index)
        {
            pvBuses.Add(new PvBus(index, RealPower, VoltageMagnitude));
        }

        public void AddTo(IList<PqBus> pqBuses, int index)
        {
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
            voltages[index] = Complex.FromPolarCoordinates(VoltageMagnitude, voltages[index].Phase);
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
            powers[index] = new Complex(RealPower, powers[index].Imaginary);
        }
    }
}
