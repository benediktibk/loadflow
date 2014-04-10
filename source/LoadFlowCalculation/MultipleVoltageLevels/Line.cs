using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Line : IPowerNetElement
    {
        private readonly string _name;
        private readonly INode _sourceNode;
        private readonly INode _targetNode;

        public Line(string name, INode sourceNode, INode targetNode)
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

        public void AddConnectedNodes(ISet<INode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }
    }
}
