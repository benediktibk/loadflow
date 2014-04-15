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

        public abstract SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower);
    }
}
