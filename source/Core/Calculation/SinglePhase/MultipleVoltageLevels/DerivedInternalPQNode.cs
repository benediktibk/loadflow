using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        private readonly Complex _power;

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, int id, Complex power, string name) : base(sourceNode, id, name)
        {
            _power = power;
        }

        protected override SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.Node() {Power = scaler.ScalePower(_power)};
        }
    }
}
