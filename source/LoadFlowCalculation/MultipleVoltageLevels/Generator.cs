using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

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

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return true; }
        }

        public PVBus CreatePVBus(IDictionary<IReadOnlyNode, int> nodeIndexes, double scaleBasisVoltage, double scaleBasisPower)
        {
            throw new NotImplementedException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasisPower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasisVoltage)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
        }
    }
}
