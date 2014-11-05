using System.Numerics;

namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PqNode : Node
    {
        public PqNode(Complex power)
        {
            Power = power;
        }
    }
}
