using System;
using System.Collections.Generic;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Line : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _sourceNode;
        private readonly IReadOnlyNode _targetNode;

        public Line(string name, IReadOnlyNode sourceNode, IReadOnlyNode targetNode)
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

        public bool NominalVoltagesMatch
        {
            get { return Math.Abs((TargetNominalVoltage - SourceNominalVoltage)/TargetNominalVoltage) < 0.0000001; }
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }
    }
}
