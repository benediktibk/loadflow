namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Generator
    {
        private readonly string _name;
        private readonly Node _node;

        public Generator(string name, Node node)
        {
            _name = name;
            _node = node;
        }

        public string Name 
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }
    }
}
