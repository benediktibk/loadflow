using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class FeedIn : IPowerNetElement
    {
        private readonly string _name;
        private readonly INode _node;
        private readonly Complex _voltage;

        public FeedIn(string name, INode node, Complex voltage)
        {
            _name = name;
            _node = node;
            _voltage = voltage;
        }

        public string Name
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public Complex Voltage
        {
            get { return _voltage; }
        }

        public void AddConnectedNodes(ISet<INode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
