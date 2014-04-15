using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        private readonly Complex _power;

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, string name, Complex power) : base(sourceNode, name)
        {
            _power = power;
        }

        public override SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node() {Power = scaler.ScalePower(_power)};
        }
    }
}
