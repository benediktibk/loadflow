using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class NodeWithIndex
    {
        public NodeWithIndex(INode node, int index)
        {
            Node = node;
            Index = index;
        }

        public INode Node { get; private set; }

        public int Index { get; private set; }

        public void SetVoltageIn(Vector<Complex> voltages)
        {
            Node.SetVoltageIn(voltages, Index);
        }

        public void SetVoltageMagnitudeIn(Vector<Complex> voltages)
        {
            Node.SetVoltageMagnitudeIn(voltages, Index);
        }

        public void SetPowerIn(Vector<Complex> powers)
        {
            Node.SetPowerIn(powers, Index);
        }

        public void SetRealPowerIn(Vector<Complex> powers)
        {
            Node.SetRealPowerIn(powers, Index);
        }
    }
}
