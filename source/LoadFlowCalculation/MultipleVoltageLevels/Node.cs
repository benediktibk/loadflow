using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Node
    {
        private readonly string _name;
        private readonly double _nominalVoltage;
        private readonly List<IPowerNetElement> _connectedElements; 

        public Node(string name, double nominalVoltage)
        {
            _name = name;
            _nominalVoltage = nominalVoltage;
            _connectedElements = new List<IPowerNetElement>();
        }

        public void Connect(IPowerNetElement element)
        {
            _connectedElements.Add(element);
        }

        public IReadOnlyCollection<IPowerNetElement> ConnectedElements
        {
            get { return _connectedElements; }
        }

        public double NominalVoltage
        {
            get { return _nominalVoltage; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}
