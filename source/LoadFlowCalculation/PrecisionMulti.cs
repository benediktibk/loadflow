namespace LoadFlowCalculation
{
    public class PrecisionMulti : Precision
    {
        public PrecisionMulti(int bitPrecision) :
            base(DataType.MultiPrecision, bitPrecision)
        { }
    }
}
