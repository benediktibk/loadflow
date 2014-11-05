namespace Calculation.SinglePhase.SingleVoltageLevel
{
    public class PvNode : Node
    {
        public PvNode(double realPower, double voltageMagnitude)
        {
            RealPower = realPower;
            VoltageMagnitude = voltageMagnitude;
        }
    }
}
