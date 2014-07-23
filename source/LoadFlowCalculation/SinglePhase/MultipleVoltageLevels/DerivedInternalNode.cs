namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNode : IReadOnlyNode
    {
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly long _id;

        protected DerivedInternalNode(IExternalReadOnlyNode sourceNode, long id)
        {
            _sourceNode = sourceNode;
            _id = id;
        }

        public bool Equals(IReadOnlyNode other)
        {
            return Id == other.Id;
        }

        public double NominalVoltage 
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public long Id
        {
            get { return _id; }
        }

        public abstract SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
    }
}
