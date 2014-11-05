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

        public void AddTo(IList<NodeWithIndex> slackNodes, IList<NodeWithIndex> pqNodes, IList<NodeWithIndex> pvNodes, int index)
        {
            pvNodes.Add(new NodeWithIndex(this, index));
        }

        public void AddTo(IList<PvNodeWithIndex> pvBuses, int index)
        {
            pvBuses.Add(new PvNodeWithIndex(index, RealPower, VoltageMagnitude));
        }

        public void AddTo(IList<PqNodeWithIndex> pqBuses, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
            voltages[index] = Complex.FromPolarCoordinates(VoltageMagnitude, voltages[index].Phase);
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
            powers[index] = new Complex(RealPower, powers[index].Imaginary);
        }
    }
}
