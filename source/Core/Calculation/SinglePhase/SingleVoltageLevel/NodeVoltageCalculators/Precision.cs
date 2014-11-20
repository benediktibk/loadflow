using System;

namespace Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    public enum DataType
    {
        LongDouble,
        MultiPrecision
    };

    public class Precision
    {
        private readonly DataType _dataType;
        private readonly int _bits;

        protected Precision(DataType dataType, int bits)
        {
            if (dataType == DataType.MultiPrecision && bits <= 0)
                throw new ArgumentOutOfRangeException("bits", "must be greater than 0");

            _dataType = dataType;
            _bits = bits;
        }

        public DataType Type
        {
            get { return _dataType; }
        }

        public int Bits
        {
            get
            {
                return _bits;
            }
        }

        public static Precision Create(int bitPrecision)
        {
            if (bitPrecision <= 64)
                return new PrecisionLongDouble();
            else
                return new PrecisionMulti(bitPrecision);
        }
    }
}
