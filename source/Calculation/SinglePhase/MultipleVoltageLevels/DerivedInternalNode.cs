namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public abstract class DerivedInternalNode : IReadOnlyNode
    {
        #region variables

        private readonly IExternalReadOnlyNode _sourceNode;

        #endregion

        #region constructor

        protected DerivedInternalNode(IExternalReadOnlyNode sourceNode, int id)
        {
            _sourceNode = sourceNode;
            Id = id;
        }

        #endregion

        #region properties

        public double NominalVoltage 
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public int Id { get; private set; }

        #endregion

        #region public functions

        public bool Equals(IReadOnlyNode other)
        {
            return Id == other.Id;
        }

        public SingleVoltageLevel.Node CreateSingleVoltageNode(double scaleBasePower)
        {
            var node = CreateSingleVoltageNodeInternal(scaleBasePower);
            node.NominalPhaseShift = _sourceNode.NominalPhaseShift;
            return node;
        }

        #endregion

        #region abstract functions

        protected abstract SingleVoltageLevel.Node CreateSingleVoltageNodeInternal(double scaleBasePower);

        #endregion
    }
}
