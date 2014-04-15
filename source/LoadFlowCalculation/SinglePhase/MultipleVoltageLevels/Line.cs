using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class Line : IPowerNetElement
    {
        private readonly string _name;
        private readonly IExternalReadOnlyNode _sourceNode;
        private readonly IExternalReadOnlyNode _targetNode;
        private readonly Complex _lengthImpedance;

        public Line(string name, IExternalReadOnlyNode sourceNode, IExternalReadOnlyNode targetNode, double lengthResistance, double lengthInductance, double frequency)
        {
            _name = name;
            _sourceNode = sourceNode;
            _targetNode = targetNode;
            _lengthImpedance = new Complex(lengthResistance, 2*Math.PI*frequency*lengthInductance);
        }

        public string Name
        {
            get { return _name; }
        }

        public Complex LengthImpedance
        {
            get { return _lengthImpedance; }
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
            _sourceNode.AddConnectedNodes(visitedNodes);
            _targetNode.AddConnectedNodes(visitedNodes);
        }

        public void FillInAdmittances(Matrix admittances, IReadOnlyDictionary<IExternalReadOnlyNode, int> nodeIndexes, double scaleBasisPower)
        {
            var scaler = new DimensionScaler(TargetNominalVoltage, scaleBasisPower);
            var sourceIndex = nodeIndexes[_sourceNode];
            var targetIndex = nodeIndexes[_targetNode];
            var admittance = scaler.ScaleAdmittance(1.0/LengthImpedance);
            admittances[sourceIndex, sourceIndex] += admittance;
            admittances[targetIndex, targetIndex] += admittance;
            admittances[sourceIndex, targetIndex] -= admittance;
            admittances[targetIndex, sourceIndex] -= admittance;
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }
    }
}
