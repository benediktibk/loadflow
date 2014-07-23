namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPVNode : DerivedInternalNode
    {
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        public DerivedInternalPVNode(IExternalReadOnlyNode sourceNode, long id, double voltageMagnitude, double realPower) : base(sourceNode, id)
        {
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        public override SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node()
            {
                VoltageMagnitude = scaler.ScaleVoltage(_voltageMagnitude),
                RealPower = scaler.ScalePower(_realPower)
            };
        }
    }
}
