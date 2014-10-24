using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class ImpedanceLoad : IPowerNetElement
    {
        #region variables

        private readonly IExternalReadOnlyNode _node;
        private Complex _impedance;

        #endregion

        #region constructor

        public ImpedanceLoad(IExternalReadOnlyNode node, Complex impedance)
        {
            _node = node;
            _impedance = impedance;
        }

        #endregion

        #region properties

        public double NominalVoltage
        {
            get { return _node.NominalVoltage; }
        }

        public Complex Impedance
        {
            get { return _impedance; }
        }

        #endregion

        #region IPowerNetElement

        public void AddConnectedNodes(ISet<IExternalReadOnlyNode> visitedNodes)
        {
            _node.AddConnectedNodes(visitedNodes);
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
            return new Complex(0, 0);
        }

        public Complex GetSlackVoltage(double scaleBasePower)
        {
            throw new InvalidOperationException();
        }

        public IList<IReadOnlyNode> GetInternalNodes()
        {
            return new List<IReadOnlyNode>();
        }

        public void FillInAdmittances(IAdmittanceMatrix admittances, double scaleBasisPower, IReadOnlyNode groundNode,
            double expectedLoadFlow)
        {
            var scaler = new DimensionScaler(NominalVoltage, scaleBasisPower);
            var impedanceScaled = scaler.ScaleImpedance(_impedance);
            var admittanceScaled = 1.0/impedanceScaled;
            admittances.AddConnection(_node, groundNode, admittanceScaled);
        }

        public bool NominalVoltagesMatch
        {
            get { return true; }
        }

        public bool NeedsGroundNode
        {
            get { return true; }
        }

        #endregion
    }
}
