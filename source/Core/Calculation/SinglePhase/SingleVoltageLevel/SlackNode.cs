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

        public void AddTo(IList<NodeWithIndex> slackNodes, IList<NodeWithIndex> pqNodes, IList<NodeWithIndex> pvNodes, int index)
        {
            slackNodes.Add(new NodeWithIndex(this, index));
        }

        public void AddTo(IList<PvNodeWithIndex> pvBuses, int index)
        {
            throw new InvalidOperationException();
        }

        public void AddTo(IList<PqNodeWithIndex> pqBuses, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetVoltageIn(Vector<Complex> voltages, int index)
        {
            voltages[index] = Voltage;
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetPowerIn(Vector<Complex> powers, int index)
        {
            throw new InvalidOperationException();
        }

        public void SetRealPowerIn(Vector<Complex> powers, int index)
        {
            throw new InvalidOperationException();
        }
    }
}
