namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class PrecisionMulti : Precision
    {
        public PrecisionMulti(int bits) :
            base(DataType.MultiPrecision, bits)
        { }
    }
}
