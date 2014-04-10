using System.Collections.Generic;
using System.Numerics;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Load : IPowerNetElement
    {
        private readonly Complex _load;
        private readonly string _name;
        private readonly IReadOnlyNode _node;

        public Load(string name, Complex load, IReadOnlyNode node)
        {
            _load = load;
            _name = name;
            _node = node;
        }

        public Complex Value
        {
            get { return _load; }
        }

        public string Name
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
