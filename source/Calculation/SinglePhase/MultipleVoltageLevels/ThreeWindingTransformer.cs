using System;
using System.Collections.Generic;
using System.Numerics;
using MathExtensions;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ThreeWindingTransformer : IPowerNetElement
    {
        #region variables

        private readonly IExternalReadOnlyNode _nodeOne;
        private readonly IExternalReadOnlyNode _nodeTwo;
        private readonly IExternalReadOnlyNode _nodeThree;
        private readonly Complex _shuntAdmittance;
        private readonly Complex _lengthAdmittanceOne;
        private readonly Complex _lengthAdmittanceTwo;
        private readonly Complex _lengthAdmittanceThree;
        private readonly double _averageNominalPower;
        private readonly Angle _nominalPhaseShiftOneToTwo;
        private readonly Angle _nominalPhaseShiftTwoToThree;
        private readonly Angle _nominalPhaseShiftThreeToOne;

        #endregion

        #region constructor

        public ThreeWindingTransformer(
            IExternalReadOnlyNode nodeOne, IExternalReadOnlyNode nodeTwo, IExternalReadOnlyNode nodeThree,
            double nominalPowerOne, double nominalPowerTwo, double nominalPowerThree,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree,
            double relativeShortCircuitVoltageThreeToOne,
            double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne,
            double ironLosses, double relativeNoLoadCurrent,
            Angle nominalPhaseShiftOneToTwo, Angle nominalPhaseShiftTwoToThree, Angle nominalPhaseShiftThreeToOne,
            string name, IdGenerator idGenerator)
        {
            
        }

        #endregion

        #region IPowerNetElement

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            throw new NotImplementedException();
        }

        public void AddConnectedNodesOnSameVoltageLevel(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            throw new NotImplementedException();
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

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            throw new NotImplementedException();
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode,
            double expectedLoadFlow)
        {
            throw new NotImplementedException();
        }

        public bool NominalVoltagesMatch
        {
            get { throw new NotImplementedException(); }
        }

        public bool NeedsGroundNode
        {
            get { return true; }
        }

        #endregion
    }
}
