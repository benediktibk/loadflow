using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        private readonly Complex _power;

        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, int id, Complex power, string name) : base(sourceNode, id, name)
        {
            _power = power;
        }

        public override INode CreateSingleVoltageNode(double scaleBasePower, ISet<IExternalReadOnlyNode> visited)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new PqNode(scaler.ScalePower(_power));
        }
    }
}
