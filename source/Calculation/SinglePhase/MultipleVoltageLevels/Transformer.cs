using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Transformer : IPowerNetElement
    {
        #region variables

        private readonly IExternalReadOnlyNode _upperSideNode;
        private readonly IExternalReadOnlyNode _lowerSideNode;
        private Complex _lengthAdmittance;
        private Complex _shuntAdmittance;
        private readonly double _ratio;
        private readonly double _nominalPower;
        private readonly double _nominalPhaseShift;
        private readonly List<DerivedInternalPQNode> _internalNodes;

        #endregion

        #region public functions

        public Transformer(IExternalReadOnlyNode upperSideNode, IExternalReadOnlyNode lowerSideNode, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, double nominalPhaseShift, string name, IdGenerator idGenerator)
        {
            if (upperSideNode == null)
                throw new ArgumentOutOfRangeException("upperSideNode", "must not be null");

            if (lowerSideNode == null)
                throw new ArgumentOutOfRangeException("lowerSideNode", "must not be null");

            if (ratio <= 0)
                throw new ArgumentOutOfRangeException("ratio", "must be positive");

            if (nominalPower <= 0)
                throw new ArgumentOutOfRangeException("nominalPower", "must be positive");

            if (relativeShortCircuitVoltage <= 0 || relativeShortCircuitVoltage > 1)
                throw new ArgumentOutOfRangeException("relativeShortCircuitVoltage", "must be positive and smaller than 1");

            if (copperLosses <= 0)
                throw new ArgumentOutOfRangeException("copperLosses", "must be positive");

            if (relativeNoLoadCurrent < 0 || relativeNoLoadCurrent > 1)
                throw new ArgumentOutOfRangeException("relativeNoLoadCurrent", "must be positive and smaller than 1");

            if (ironLosses <= 0 && relativeNoLoadCurrent == 0)
                throw new ArgumentOutOfRangeException("ironLosses", "must be positive");

            if (nominalPhaseShift < 0 || nominalPhaseShift > 2*Math.PI)
                throw new ArgumentOutOfRangeException("nominalPhaseShift", "must be within 0 and 2*pi");

            _upperSideNode = upperSideNode;
            _lowerSideNode = lowerSideNode;
            _ratio = ratio;
            _nominalPower = nominalPower;
            _nominalPhaseShift = nominalPhaseShift;
            _internalNodes = new List<DerivedInternalPQNode>();
            CalculateAdmittances(nominalPower, relativeShortCircuitVoltage, copperLosses, ironLosses,
                relativeNoLoadCurrent);

            if (!HasNotNominalRatio) 
                return;

            _internalNodes.Add(new DerivedInternalPQNode(upperSideNode, idGenerator.Generate(), new Complex(0, 0), name + "#upper"));
            _internalNodes.Add(new DerivedInternalPQNode(lowerSideNode, idGenerator.Generate(), new Complex(0, 0), name + "#lower"));
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

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            if (visitedNodes.Contains(_upperSideNode))
                _upperSideNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            else if (visitedNodes.Contains(_lowerSideNode))
                _lowerSideNode.AddConnectedNodesOnSameVoltageLevel(visitedNodes);
            else
                throw new ArgumentException("neither the upper side nor the lower side node have been visited yet");
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        {
            var scalerUpperSide = new DimensionScaler(UpperSideNominalVoltage, scaleBasisPower);

            if (HasNotNominalRatio)
            {
                var idealTransformerWeight = expectedLoadFlow == 0 ? 1 : UpperSideNominalVoltage / _nominalPower;
                var idealTransformerUpperSideNode = _internalNodes[0];
                var idealTransformerInternalNode = _internalNodes[1];
                var lengthAdmittanceScaled = scalerUpperSide.ScaleAdmittance(LengthAdmittance);
                var shuntAdmittanceScaled = scalerUpperSide.ScaleAdmittance(ShuntAdmittance);
                admittances.AddConnection(_upperSideNode, groundNode, shuntAdmittanceScaled);
                admittances.AddConnection(idealTransformerUpperSideNode, groundNode, shuntAdmittanceScaled);
                admittances.AddConnection(_upperSideNode, idealTransformerUpperSideNode, lengthAdmittanceScaled);
                admittances.AddIdealTransformer(idealTransformerUpperSideNode, groundNode, _lowerSideNode, groundNode,
                    idealTransformerInternalNode, RelativeRatio, idealTransformerWeight);
            }
            else
            {
                var lengthAdmittanceScaled = scalerUpperSide.ScaleAdmittance(LengthAdmittance);
                var shuntAdmittanceScaled = scalerUpperSide.ScaleAdmittance(ShuntAdmittance);
                admittances.AddConnection(_upperSideNode, groundNode, shuntAdmittanceScaled);
                admittances.AddConnection(_lowerSideNode, groundNode, shuntAdmittanceScaled);
                admittances.AddConnection(_upperSideNode, _lowerSideNode, lengthAdmittanceScaled);
            }
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return _internalNodes.Cast<IReadOnlyNode>().ToList();
        }

        #endregion

        #region private functions

        private void CalculateAdmittances(double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent)
        {
            var relativeShortCircuitVoltageReal = copperLosses / nominalPower;
            if (relativeShortCircuitVoltageReal > relativeShortCircuitVoltage)
                throw new ArgumentException("the copper losses are too high compared to the nominal power");

            var relativeShortCircuitVoltageImaginary =
                Math.Sqrt(relativeShortCircuitVoltage * relativeShortCircuitVoltage -
                          relativeShortCircuitVoltageReal * relativeShortCircuitVoltageReal);
            var relativeShortCircuitVoltageComplex = new Complex(relativeShortCircuitVoltageReal,
                relativeShortCircuitVoltageImaginary);
            _lengthAdmittance = (nominalPower / (UpperSideNominalVoltage * UpperSideNominalVoltage)) / relativeShortCircuitVoltageComplex;
            _shuntAdmittance = new Complex(ironLosses, (-1) * relativeNoLoadCurrent * nominalPower) / (2 * UpperSideNominalVoltage * UpperSideNominalVoltage);
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

        public Complex LengthAdmittance
        {
            get { return _lengthAdmittance; }
        }

        public Complex ShuntAdmittance
        {
            get { return _shuntAdmittance; }
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
            get { return true; }
        }

        public bool HasNotNominalRatio
        {
            get { return Math.Abs(RelativeRatio - 1) > 0.000001; } 
        }

        public double NominalPhaseShift
        {
            get { return _nominalPhaseShift; }
        }

        public IExternalReadOnlyNode UpperSideNode
        {
            get { return _upperSideNode; }
        }

        public IExternalReadOnlyNode LowerSideNode
        {
            get { return _lowerSideNode; }
        }

        #endregion
    }
}
