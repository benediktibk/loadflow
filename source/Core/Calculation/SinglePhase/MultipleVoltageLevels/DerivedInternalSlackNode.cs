using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalSlackNode : DerivedInternalNode
    {
        private readonly Complex _voltage;

        public DerivedInternalSlackNode(IExternalReadOnlyNode sourceNode, int id, Complex voltage, string name) : base(sourceNode, id, name)
        {
            _voltage = voltage;
        }

        protected override INode CreateSingleVoltageNodesInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SlackNode(scaler.ScaleVoltage(_voltage));
        }
    }
}
