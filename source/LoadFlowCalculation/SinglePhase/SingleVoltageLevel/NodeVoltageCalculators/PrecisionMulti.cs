namespace LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public class PrecisionMulti : Precision
    {
        public PrecisionMulti(int bitPrecision) :
            base(DataType.MultiPrecision, bitPrecision)
        { }
    }
}
