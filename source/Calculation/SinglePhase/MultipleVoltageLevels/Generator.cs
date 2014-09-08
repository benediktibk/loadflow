using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class Generator : IPowerNetElement
    {
        #region variables

        private readonly IExternalReadOnlyNode _node;
        private readonly double _voltageMagnitude;
        private readonly double _realPower;

        #endregion

        #region constructor

        public Generator(IExternalReadOnlyNode node, double voltageMagnitude, double realPower)
        {
            _node = node;
            _voltageMagnitude = voltageMagnitude;
            _realPower = realPower;
        }

        #endregion

        #region properties

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

        public bool NeedsGroundNode
        {
            get { return false; }
        }

        #endregion

        #region public functions

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

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode, double expectedLoadFlow)
        { }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }

        #endregion
    }
}
