using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Transformator : IPowerNetElement
    {
        private readonly string _name;
        private readonly double _nominalPower;
        private readonly double _shortCircuitVoltageInPercentage;
        private readonly double _copperLosses;
        private readonly double _ironLosses;
        private readonly double _alpha;
        private readonly IReadOnlyNode _upperSideNode;
        private readonly IReadOnlyNode _lowerSideNode;

        public Transformator(string name, double nominalPower, double shortCircuitVoltageInPercentage, double copperLosses, double ironLosses, double alpha, IReadOnlyNode upperSideNode, IReadOnlyNode lowerSideNode)
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

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasisPower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasisPower)
        {
            return new Complex();
        }

        public Complex GetSlackVoltage()
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _upperSideNode.AddConnectedNodes(visitedNodes);
            _lowerSideNode.AddConnectedNodes(visitedNodes);
        }
    }
}
