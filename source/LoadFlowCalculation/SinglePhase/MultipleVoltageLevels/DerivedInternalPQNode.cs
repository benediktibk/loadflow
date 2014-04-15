using System;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class DerivedInternalPQNode : DerivedInternalNode
    {
        public DerivedInternalPQNode(IExternalReadOnlyNode sourceNode, string name) : base(sourceNode, name)
        {
        }

        public override bool MustBeSlackBus
        {
            get { throw new NotImplementedException(); }
        }

        public override bool MustBePVBus
        {
            get { throw new NotImplementedException(); }
        }

        public override Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            throw new NotImplementedException();
        }

        public override Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            throw new NotImplementedException();
        }

        public override Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new NotImplementedException();
        }
    }
}
