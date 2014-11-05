using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class SlackNode : Node
    {
        public SlackNode(Complex voltage)
        {
            Voltage = voltage;
        }
    }
}
