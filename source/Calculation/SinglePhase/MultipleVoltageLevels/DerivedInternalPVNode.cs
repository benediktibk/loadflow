namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPVNode : DerivedInternalNode
    {
        #region variables

        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        #endregion

        #region constructor

        public DerivedInternalPVNode(IExternalReadOnlyNode sourceNode, int id, double voltageMagnitude, double realPower) : base(sourceNode, id)
        {
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        #endregion

        #region protected functions

        protected override SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node()
            {
                VoltageMagnitude = scaler.ScaleVoltage(_voltageMagnitude),
                RealPower = scaler.ScalePower(_realPower)
            };
        }

        #endregion
    }
}
