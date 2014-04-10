namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Transformator
    {
        private readonly string _name;
        private readonly double _nominalPower;
        private readonly double _shortCircuitVoltageInPercentage;
        private readonly double _copperLosses;
        private readonly double _ironLosses;
        private readonly double _alpha;
        private readonly INode _upperSideNode;
        private readonly INode _lowerSideNode;

        public Transformator(string name, double nominalPower, double shortCircuitVoltageInPercentage, double copperLosses, double ironLosses, double alpha, INode upperSideNode, INode lowerSideNode)
        {
            _name = name;
            _nominalPower = nominalPower;
            _shortCircuitVoltageInPercentage = shortCircuitVoltageInPercentage;
            _copperLosses = copperLosses;
            _ironLosses = ironLosses;
            _alpha = alpha;
            _upperSideNode = upperSideNode;
            _lowerSideNode = lowerSideNode;
        }

        public string Name
        {
            get { return _name; }
        }

        public double UpperSideNominalVoltage
        {
            get { return _upperSideNode.NominalVoltage; }
        }

        public double LowerSideNominalVoltage
        {
            get { return _lowerSideNode.NominalVoltage; }
        }
    }
}
