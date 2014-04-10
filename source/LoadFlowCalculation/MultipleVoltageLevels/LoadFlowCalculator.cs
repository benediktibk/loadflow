using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using LoadFlowCalculation.SingleVoltageLevel.NodeVoltageCalculators;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class LoadFlowCalculator
    {
        #region variables

        private readonly double _scaleBasisVoltage;
        private readonly double _scaleBasisPower;
        private readonly double _scaleBasisCurrent;
        private readonly double _scaleBasisImpedance;
        private readonly INodeVoltageCalculator _nodeVoltageCalculator;

        #endregion

        #region public functions

        public LoadFlowCalculator(double scaleBasisVoltage, double scaleBasisPower, INodeVoltageCalculator nodeVoltageCalculator)
        {
            _scaleBasisVoltage = scaleBasisVoltage;
            _scaleBasisPower = scaleBasisPower;
            _scaleBasisCurrent = scaleBasisPower/scaleBasisVoltage;
            _scaleBasisImpedance = _scaleBasisVoltage/_scaleBasisCurrent;
            _nodeVoltageCalculator = nodeVoltageCalculator;
        }

        public IDictionary<string, Complex> CalculateNodeVoltages(IReadOnlyPowerNet powerNet)
        {
            if (powerNet.CheckIfFloatingNodesExists())
                throw new ArgumentOutOfRangeException("powerNet", "there must not be a floating node");
            if (powerNet.CheckIfNominalVoltagesDoNotMatch())
                throw new ArgumentOutOfRangeException("powerNet", "the nominal voltages must match on connected nodes");

            var calculator = new SingleVoltageLevel.LoadFlowCalculator(_nodeVoltageCalculator);
            var nodeVoltages = new Dictionary<string, Complex>();
            return nodeVoltages;
        }

        #endregion

        #region public properties

        public double ScaleBasisVoltage
        {
            get { return _scaleBasisVoltage; }
        }

        public double ScaleBasisPower
        {
            get { return _scaleBasisPower; }
        }

        public double ScaleBasisCurrent
        {
            get { return _scaleBasisCurrent; }
        }

        public double ScaleBasisImpedance
        {
            get { return _scaleBasisImpedance; }
        }

        #endregion

        #region private functions
        #endregion
    }
}
