using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathExtensions;

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

        public void AddNode(int id, double nominalVoltage, string name)
        {
            _singlePhasePowerNet.AddNode(id, nominalVoltage/Math.Sqrt(3), 0, name);
        }

        public void AddTransmissionLine(int sourceNodeId, int targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel)
        {
            _singlePhasePowerNet.AddTransmissionLine(sourceNodeId, targetNodeId, seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntConductancePerUnitLength, shuntCapacityPerUnitLength, length, transmissionEquationModel);
        }

        public void AddGenerator(int nodeId, double voltageMagnitude, double realPower)
        {
            _singlePhasePowerNet.AddGenerator(nodeId, voltageMagnitude/Math.Sqrt(3), realPower/3);
        }

        public void AddFeedIn(int nodeId, Complex voltage, double shortCircuitPower, double c, double realToImaginary)
        {
            _singlePhasePowerNet.AddFeedIn(nodeId, voltage/Math.Sqrt(3), shortCircuitPower/3, c, realToImaginary);
        }

        public void AddTwoWindingTransformer(int upperSideNodeId, int lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name)
        {
            _singlePhasePowerNet.AddTwoWindingTransformer(upperSideNodeId, lowerSideNodeId, nominalPower/3,
                relativeShortCircuitVoltage, copperLosses/3, ironLosses/3, relativeNoLoadCurrent, ratio, nominalPhaseShift, name);
        }

        public void AddLoad(int nodeId, Complex power)
        {
            _singlePhasePowerNet.AddLoad(nodeId, power/3);
        }

        public void AddImpedanceLoad(int nodeId, Complex impedance)
        {
            _singlePhasePowerNet.AddImpedanceLoad(nodeId, impedance);
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

        public Complex GetNodePower(long nodeId)
        {
            var node = _singlePhasePowerNet.GetNodeById(nodeId);
            return node.Power * 3;
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerBase)
        {
            _singlePhasePowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);
        }

        public Angle GetSlackPhaseShift()
        {
            return _singlePhasePowerNet.GetSlackPhaseShift();
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> GetNominalPhaseShiftPerNode()
        {
            return _singlePhasePowerNet.GetNominalPhaseShiftPerNode();
        }

        #endregion
    }
}
