using System;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, string name) : base(sourceNode, name)
        {
        }

        public override SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            throw new NotImplementedException();
        }
    }
}
