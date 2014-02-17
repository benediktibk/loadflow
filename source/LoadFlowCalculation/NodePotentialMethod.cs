namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        LoadFlowCalculator
    {
        public NodePotentialMethod() :
            base(new NodePotentialMethodInternal())
        { }
    }
}
