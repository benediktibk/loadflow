using System;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Calculation.ThreePhase
{
    public class SymmetricPowerNet
    {
        #region variables

        private readonly PowerNet _singlePhasePowerNet;

        #endregion

        #region constructor

        public SymmetricPowerNet(double frequency)
        {
            _singlePhasePowerNet = new PowerNet(frequency);
        }

        #endregion

        #region add functions

        public void AddNode(int id, double nominalVoltage)
        {
            _singlePhasePowerNet.AddNode(id, nominalVoltage/Math.Sqrt(3), 0);
        }

        public void AddLine(int sourceNodeId, int targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length)
        {
            _singlePhasePowerNet.AddLine(sourceNodeId, targetNodeId, seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntConductancePerUnitLength, shuntCapacityPerUnitLength, length);
        }

        public void AddGenerator(int nodeId, double voltageMagnitude, double realPower)
        {
            _singlePhasePowerNet.AddGenerator(nodeId, voltageMagnitude/Math.Sqrt(3), realPower/3);
        }

        public void AddFeedIn(int nodeId, Complex voltage, double shortCircuitPower)
        {
            _singlePhasePowerNet.AddFeedIn(nodeId, voltage/Math.Sqrt(3), shortCircuitPower/3);
        }

        public void AddTransformer(int upperSideNodeId, int lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, int phaseShift)
        {
            _singlePhasePowerNet.AddTransformer(upperSideNodeId, lowerSideNodeId, nominalPower/3,
                relativeShortCircuitVoltage, copperLosses/3, ironLosses/3, relativeNoLoadCurrent, ratio, phaseShift);
        }

        public void AddLoad(int nodeId, Complex power)
        {
            _singlePhasePowerNet.AddLoad(nodeId, power/3);
        }

        #endregion

        #region calculations

        public bool CalculateNodeVoltages(INodeVoltageCalculator nodeVoltageCalculator)
        {
            return _singlePhasePowerNet.CalculateNodeVoltages(nodeVoltageCalculator);
        }

        public Complex GetNodeVoltage(long nodeId)
        {
            var node = _singlePhasePowerNet.GetNodeById(nodeId);
            return node.Voltage*Math.Sqrt(3);
        }

        #endregion
    }
}
