using System;

namespace LoadFlowCalculationComparison.AlgorithmSettings
{
    public class HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy : HolomorphicEmbeddedLoadFlowMethodSettings
    {
        private int _bitPrecision = 300;

        public HolomorphicEmbeddedLoadFlowMethodSettingsHighAccuracy(GeneralSettings generalSettings) : base(generalSettings)
        { }

        public int BitPrecision
        {
            get { return _bitPrecision; }
            set
            {
                if (value == _bitPrecision)
                    return;

                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "must be greater 0");

                _bitPrecision = value;
                OnPropertyChanged();
            }
        }
    }
}
