using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class Line : IPowerNetElement
    {
        private readonly string _name;
        private readonly IReadOnlyNode _sourceNode;
        private readonly IReadOnlyNode _targetNode;
        private readonly double _lengthResistance;

        public Line(string name, IReadOnlyNode sourceNode, IReadOnlyNode targetNode, double lengthResistance)
        {
            _name = name;
            _sourceNode = sourceNode;
            _targetNode = targetNode;
            _lengthResistance = lengthResistance;
        }

        public string Name
        {
            get { return _name; }
        }

        public double LengthResistance
        {
            get { return _lengthResistance; }
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

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return false; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasisVoltage, double scaleBasisPower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetTotalPowerForPQBus(double scaleBasisPower)
        {
            return new Complex();
        }

        public Complex GetSlackVoltage(double scaleBasisVoltage)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IReadOnlyNode> visitedNodes)
        {
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(Matrix admittances, IDictionary<IReadOnlyNode, int> nodeIndexes,
            double scaleBasisImpedance)
        {
            var sourceIndex = nodeIndexes[_sourceNode];
            var targetIndex = nodeIndexes[_targetNode];
            var lengthResistanceScaled = LengthResistance/scaleBasisImpedance;
            var lengthAdmittanceScaled = 1/lengthResistanceScaled;
            admittances[sourceIndex, sourceIndex] += lengthAdmittanceScaled;
            admittances[targetIndex, targetIndex] += lengthAdmittanceScaled;
            admittances[sourceIndex, targetIndex] -= lengthAdmittanceScaled;
            admittances[targetIndex, sourceIndex] -= lengthAdmittanceScaled;
        }
    }
}
