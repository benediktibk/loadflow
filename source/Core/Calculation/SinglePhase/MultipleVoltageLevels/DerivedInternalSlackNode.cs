using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalSlackNode : DerivedInternalNode
    {
        private readonly Complex _voltage;

        public DerivedInternalSlackNode(IExternalReadOnlyNode sourceNode, int id, Complex voltage, string name) : base(sourceNode, id, name)
        {
            _voltage = voltage;
        }

        protected override SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.SlackNode(scaler.ScaleVoltage(_voltage));
        }
    }
}
