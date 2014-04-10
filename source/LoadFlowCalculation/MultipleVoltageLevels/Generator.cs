using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Generator : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _node;

        public Generator(string name, IReadOnlyNode node)
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

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
