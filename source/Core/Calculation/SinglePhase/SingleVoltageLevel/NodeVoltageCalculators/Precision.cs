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
        #region variables

        private readonly DataType _dataType;
        private readonly int _bitPrecision;

        #endregion

        #region constructor

        protected Precision(DataType dataType, int bitPrecision)
        {
            if (dataType == DataType.MultiPrecision && bitPrecision <= 0)
                throw new ArgumentOutOfRangeException("bitPrecision", "must be greater than 0");

            _dataType = dataType;
            _bitPrecision = bitPrecision;
        }

        #endregion

        #region properties

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

        #endregion

        #region static functions

        public static Precision Create(int bitPrecision)
        {
            if (bitPrecision <= 64)
                return new PrecisionLongDouble();
            else
                return new PrecisionMulti(bitPrecision);
        }

        #endregion
    }
}
