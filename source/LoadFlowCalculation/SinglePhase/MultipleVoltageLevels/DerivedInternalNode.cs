using System;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNode : IReadOnlyNode
    {
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly string _name;

        protected DerivedInternalNode(IExternalReadOnlyNode sourceNode, string name)
        {
            _sourceNode = sourceNode;
            _name = name;
        }

        public bool Equals(IReadOnlyNode other)
        {
            return Name == other.Name;
        }

        public double NominalVoltage 
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public string Name
        {
            get { return _name; }
        }

        public abstract bool MustBeSlackBus { get; }
        public abstract bool MustBePVBus { get; }
        public abstract Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower);
        public abstract Complex GetTotalPowerForPQBus(double scaleBasePower);
        public abstract Complex GetSlackVoltage(double scaleBasePower);
    }
}
