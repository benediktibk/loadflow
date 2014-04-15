using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class Generator : IPowerNetElement
    {
        private readonly string _name;
        private readonly IExternalReadOnlyNode _node;
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        public Generator(string name, IExternalReadOnlyNode node, double voltageMagnitude, double realPower)
        {
            _name = name;
            _node = node;
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        public string Name 
        {
            get { return _name; }
        }

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public double VoltageMagnitude
        {
            get { return _voltageMagnitude; }
        }

        public double RealPower
        {
            get { return _realPower; }
        }

        public bool EnforcesSlackBus
        {
            get { return false; }
        }

        public bool EnforcesPVBus
        {
            get { return true; }
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public Tuple<double, double> GetVoltageMagnitudeAndRealPowerForPVBus(double scaleBasePower)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasePower);
            return new Tuple<double, double>(scaler.ScaleVoltage(VoltageMagnitude), scaler.ScalePower(RealPower));
        }

        public Complex GetTotalPowerForPQBus(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
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
