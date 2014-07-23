using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalSlackNode : DerivedInternalNode
    {
        private readonly Complex _voltage;

        public DerivedInternalSlackNode(IExternalReadOnlyNode sourceNode, long id, Complex voltage) : base(sourceNode, id)
        {
            _voltage = voltage;
        }

        public override SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node() {Voltage = scaler.ScaleVoltage(_voltage)};
        }
    }
}
