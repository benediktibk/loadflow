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

        protected override INode CreateSingleVoltageNodesInternal(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new SingleVoltageLevel.PqNode(scaler.ScalePower(_power));
        }
    }
}
