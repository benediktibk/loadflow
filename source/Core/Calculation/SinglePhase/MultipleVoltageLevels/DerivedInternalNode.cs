namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNode : IReadOnlyNode
    {
        private readonly IExternalReadOnlyNode _sourceNode;

        protected DerivedInternalNode(IExternalReadOnlyNode sourceNode, int id, string name)
        {
            _sourceNode = sourceNode;
            Id = id;
            Name = name;
        }

        public double NominalVoltage 
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            return CreateSingleVoltageNodeInternal(scaleBasePower);
        }

        protected abstract SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower);
    }
}
