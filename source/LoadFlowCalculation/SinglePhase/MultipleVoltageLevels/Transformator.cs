using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class Transformator : IPowerNetElement
    {
        private readonly string _name;
        private readonly IExternalReadOnlyNode _upperSideNode;
        private readonly IExternalReadOnlyNode _lowerSideNode;

        public Transformator(string name, IExternalReadOnlyNode upperSideNode, IExternalReadOnlyNode lowerSideNode)
        {
            _name = name;
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

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            return new Complex();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _upperSideNode.AddConnectedNodes(visitedNodes);
            _lowerSideNode.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IExternalReadOnlyNode, int> nodeIndexes, double scaleBasisPower)
        {

        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}
