namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Line
    {
        private readonly string _name;
        private readonly Node _sourceNode;
        private readonly Node _targetNode;

        public Line(string name, Node sourceNode, Node targetNode)
        {
            _name = name;
            _sourceNode = sourceNode;
            _targetNode = targetNode;
        }

        public string Name
        {
            get { return _name; }
        }

        public double SourceNominalVoltage
        {
            get { return _sourceNode.NominalVoltage; }
        }

        public double TargetNominalVoltage
        {
            get { return _targetNode.NominalVoltage; }
        }
    }
}
