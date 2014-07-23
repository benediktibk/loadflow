using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LoadFlowCalculation.SinglePhase.MultipleVoltageLevels
{
    public class Transformer : IPowerNetElement
    {
        #region variables

        private readonly IExternalReadOnlyNode _upperSideNode;
        private readonly IExternalReadOnlyNode _lowerSideNode;
        private readonly Complex _upperSideImpedance;
        private readonly Complex _lowerSideImpedance;
        private readonly Complex _mainImpedance;
        private readonly double _ratio;
        private readonly List<DerivedInternalPQNode> _internalNodes;

        #endregion

        #region public functions

        public Transformer(
            IExternalReadOnlyNode upperSideNode, IExternalReadOnlyNode lowerSideNode, 
            Complex upperSideImpedance, Complex lowerSideImpedance, Complex mainImpedance, 
            double ratio, IdGenerator idGenerator)
        {
            if (upperSideNode == null)
                throw new ArgumentOutOfRangeException("upperSideNode", "must not be null");

            if (lowerSideNode == null)
                throw new ArgumentOutOfRangeException("lowerSideNode", "must not be null");

            if (ratio <= 0)
                throw new ArgumentOutOfRangeException("ratio", "must be positive");

            if (upperSideImpedance.Magnitude < 0.00001)
                throw new ArgumentOutOfRangeException("upperSideImpedance", "upper side impedance can not be zero");

            if (lowerSideImpedance.Magnitude < 0.00001)
                throw new ArgumentOutOfRangeException("lowerSideImpedance", "upper side impedance can not be zero");

            _upperSideNode = upperSideNode;
            _lowerSideNode = lowerSideNode;
            _upperSideImpedance = upperSideImpedance;
            _lowerSideImpedance = lowerSideImpedance;
            _mainImpedance = mainImpedance;
            _ratio = ratio;
            _internalNodes = new List<DerivedInternalPQNode>();

            if (HasMainImpedance || HasNotNominalRatio)
                _internalNodes.Add(new DerivedInternalPQNode(upperSideNode, idGenerator.Generate(), new Complex(0, 0)));

            if (HasNotNominalRatio)
            {
                _internalNodes.Add(new DerivedInternalPQNode(lowerSideNode, idGenerator.Generate(),
                    new Complex(0, 0)));
                _internalNodes.Add(new DerivedInternalPQNode(lowerSideNode, idGenerator.Generate(),
                    new Complex(0, 0)));
            }
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

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            var scalerUpperSide = new DimensionScaler(UpperSideNominalVoltage, scaleBasisPower);
            var scalerLowerSide = new DimensionScaler(LowerSideNominalVoltage, scaleBasisPower);
            var idealTransformerWeight = expectedLoadFlow > 0 ? UpperSideNominalVoltage / expectedLoadFlow : 1;

            if (HasMainImpedance)
            {
                if (HasNotNominalRatio)
                    FillInAdmittancesWithMainImpedanceAndNotNominalRatio(admittances, scalerUpperSide, scalerLowerSide,
                        groundNode, idealTransformerWeight);
                else
                    FillInAdmittancesWithMainImpedanceAndNominalRatio(admittances, scalerUpperSide, scalerLowerSide,
                        groundNode);
            }
            else
            {
                if (HasNotNominalRatio)
                    FillInAdmittancesWithNoMainImpedanceAndNotNominalRatio(admittances, scalerUpperSide, scalerLowerSide,
                        groundNode, idealTransformerWeight);
                else
                    FillInAdmittancesWithNoInternalNodes(admittances, scalerUpperSide, scalerLowerSide);
            }
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return _internalNodes.Cast<IReadOnlyNode>().ToList();
        }

        #endregion

        #region private functions

        private void FillInAdmittancesWithNoInternalNodes(IAdmittanceMatrix admittances, DimensionScaler scalerUpperSide, DimensionScaler scalerLowerSide)
        {
            var upperSideImpedanceScaled = scalerUpperSide.ScaleImpedance(UpperSideImpedance);
            var lowerSideImpedanceScaled = scalerLowerSide.ScaleImpedance(LowerSideImpedance);
            var admittance = 1 / (upperSideImpedanceScaled + lowerSideImpedanceScaled);
            admittances.AddConnection(_upperSideNode, _lowerSideNode, admittance);
        }

        private void FillInAdmittancesWithMainImpedanceAndNominalRatio(IAdmittanceMatrix admittances,
            DimensionScaler scalerUpperSide, DimensionScaler scalerLowerSide, IReadOnlyNode groundNode)
        {
            var mainImpedanceNode = _internalNodes[0];
            var upperSideImpedanceScaled = scalerUpperSide.ScaleImpedance(UpperSideImpedance);
            var lowerSideImpedanceScaled = scalerLowerSide.ScaleImpedance(LowerSideImpedance);
            var mainImpedanceScaled = scalerUpperSide.ScaleImpedance(MainImpedance);
            admittances.AddConnection(_upperSideNode, mainImpedanceNode, 1 / upperSideImpedanceScaled);
            admittances.AddConnection(mainImpedanceNode, _lowerSideNode, 1 / lowerSideImpedanceScaled);
            admittances.AddConnection(mainImpedanceNode, groundNode, 1 / mainImpedanceScaled);
        }

        private void FillInAdmittancesWithMainImpedanceAndNotNominalRatio(IAdmittanceMatrix admittances,
            DimensionScaler scalerUpperSide, DimensionScaler scalerLowerSide, IReadOnlyNode groundNode, double idealTransformerWeight)
        {
            var mainImpedanceNode = _internalNodes[0];
            var idealTransformerNode = _internalNodes[1];
            var idealTransformerInternalNode = _internalNodes[2];
            var upperSideImpedanceScaled = scalerUpperSide.ScaleImpedance(UpperSideImpedance);
            var lowerSideImpedanceScaled = scalerLowerSide.ScaleImpedance(LowerSideImpedance);
            var mainImpedanceScaled = scalerUpperSide.ScaleImpedance(MainImpedance);
            admittances.AddConnection(_upperSideNode, mainImpedanceNode, 1 / upperSideImpedanceScaled);
            admittances.AddConnection(idealTransformerNode, _lowerSideNode, 1 / lowerSideImpedanceScaled);
            admittances.AddConnection(mainImpedanceNode, groundNode, 1 / mainImpedanceScaled);
            admittances.AddIdealTransformer(mainImpedanceNode, groundNode, idealTransformerNode, groundNode,
                idealTransformerInternalNode, RelativeRatio, idealTransformerWeight);
        }

        private void FillInAdmittancesWithNoMainImpedanceAndNotNominalRatio(IAdmittanceMatrix admittances,
            DimensionScaler scalerUpperSide, DimensionScaler scalerLowerSide, IReadOnlyNode groundNode, double idealTransformerWeight)
        {
            var mainImpedanceNode = _internalNodes[0];
            var idealTransformerNode = _internalNodes[1];
            var idealTransformerInternalNode = _internalNodes[2];
            var upperSideImpedanceScaled = scalerUpperSide.ScaleImpedance(UpperSideImpedance);
            var lowerSideImpedanceScaled = scalerLowerSide.ScaleImpedance(LowerSideImpedance);
            admittances.AddConnection(_upperSideNode, mainImpedanceNode, 1 / upperSideImpedanceScaled);
            admittances.AddConnection(idealTransformerNode, _lowerSideNode, 1 / lowerSideImpedanceScaled);
            admittances.AddIdealTransformer(mainImpedanceNode, groundNode, idealTransformerNode, groundNode,
                idealTransformerInternalNode, RelativeRatio, idealTransformerWeight);
        }

        #endregion

        #region properties

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
            get { return HasNotNominalRatio || HasMainImpedance; }
        }

        public bool HasMainImpedance
        {
            get { return MainImpedance.Magnitude > 0; }
        }

        public bool HasNotNominalRatio
        {
            get { return Math.Abs(RelativeRatio - 1) > 0.001; } 
        }

        #endregion
    }
}
