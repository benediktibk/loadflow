using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Permissions;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class Transformer : IPowerNetElement
    {
        private readonly string _name;
        private readonly IExternalReadOnlyNode _upperSideNode;
        private readonly IExternalReadOnlyNode _lowerSideNode;
        private readonly Complex _upperSideImpedance;
        private readonly Complex _lowerSideImpedance;
        private readonly Complex _mainImpedance;
        private readonly double _ratio;

        public Transformer(
            string name, IExternalReadOnlyNode upperSideNode, IExternalReadOnlyNode lowerSideNode, 
            Complex upperSideImpedance, Complex lowerSideImpedance, Complex mainImpedance, double ratio)
        {
            if (ratio <= 0)
                throw new ArgumentOutOfRangeException("ratio", "must be positive");

            if (upperSideImpedance.Magnitude == 0)
                throw new ArgumentOutOfRangeException("upperSideImpedance", "upper side impedance can not be zero");

            if (lowerSideImpedance.Magnitude == 0)
                throw new ArgumentOutOfRangeException("lowerSideImpedance", "upper side impedance can not be zero");

            _name = name;
            _upperSideNode = upperSideNode;
            _lowerSideNode = lowerSideNode;
            _upperSideImpedance = upperSideImpedance;
            _lowerSideImpedance = lowerSideImpedance;
            _mainImpedance = mainImpedance;
            _ratio = ratio;
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

        public double NominalRatio
        {
            get { return UpperSideNominalVoltage/LowerSideNominalVoltage; }
        }

        public double Ratio
        {
            get { return _ratio; }
        }

        public double RelativeRatio
        {
            get { return Ratio/NominalRatio; }
        }

        public Complex UpperSideImpedance
        {
            get { return _upperSideImpedance; }
        }

        public Complex LowerSideImpedance
        {
            get { return _lowerSideImpedance; }
        }

        public Complex MainImpedance
        {
            get { return _mainImpedance; }
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

        public bool NeedsGroundNode
        {
            get { return MainImpedance.Magnitude > 0 || Math.Abs(RelativeRatio - 1) > 0.001; }
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

        public void FillInAdmittances(AdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode)
        {
            throw new NotImplementedException();
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            throw new NotImplementedException();
        }
    }
}
