using System;

namespace LoadFlowCalculation
{
    public enum DataType
    {
        LongDouble,
        MultiPrecision
    };

    public class Precision
    {
        private readonly DataType _dataType;
        private readonly int _bitPrecision;

        protected Precision(DataType dataType, int bitPrecision)
        {
            if (dataType == DataType.MultiPrecision && bitPrecision <= 0)
                throw new ArgumentOutOfRangeException("bitPrecision", "must be greater than 0");

            _dataType = dataType;
            _bitPrecision = bitPrecision;
        }

        public DataType Type
        {
            get { return _dataType; }
        }

        public int BitPrecision
        {
            get
            {
                if (_dataType == DataType.LongDouble)
                    throw new ArgumentOutOfRangeException();

                return _bitPrecision;
            }
        }
    }
}
