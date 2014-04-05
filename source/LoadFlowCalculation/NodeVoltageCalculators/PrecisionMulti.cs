namespace LoadFlowCalculation.NodeVoltageCalculators
{
    public class PrecisionMulti : Precision
    {
        public PrecisionMulti(int bitPrecision) :
            base(DataType.MultiPrecision, bitPrecision)
        { }
    }
}
