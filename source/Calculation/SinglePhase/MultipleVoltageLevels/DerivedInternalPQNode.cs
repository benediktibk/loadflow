using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        private readonly Complex _power;

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, int id, Complex power) : base(sourceNode, id)
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
