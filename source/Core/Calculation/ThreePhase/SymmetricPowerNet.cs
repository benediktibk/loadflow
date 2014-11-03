using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Misc;

namespace Calculation.ThreePhase
{
    public class SymmetricPowerNet
    {
        private readonly PowerNetComputable _singlePhasePowerNet;

        public SymmetricPowerNet(double frequency, INodeVoltageCalculator nodeVoltageCalculator)
        {
            _singlePhasePowerNet = new PowerNetComputable(frequency, nodeVoltageCalculator, new NodeGraph());
        }

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

        public void AddThreeWindingTransformer(long nodeOneId, long nodeTwoId, long nodeThreeId, double nominalPowerOneToTwo,
            double nominalPowerTwoToThree, double nominalPowerThreeToOne,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree,
            double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo,
            double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses,
            double relativeNoLoadCurrent, Angle nominalPhaseShiftOneToTwo,
            Angle nominalPhaseShiftTwoToThree, Angle nominalPhaseShiftThreeToOne, string name)
        {
            _singlePhasePowerNet.AddThreeWindingTransformer(nodeOneId, nodeTwoId, nodeThreeId, nominalPowerOneToTwo/3, nominalPowerTwoToThree/3,
                nominalPowerThreeToOne/3, relativeShortCircuitVoltageOneToTwo, relativeShortCircuitVoltageTwoToThree,
                relativeShortCircuitVoltageThreeToOne, copperLossesOneToTwo/3, copperLossesTwoToThree/3, copperLossesThreeToOne/3,
                ironLosses/3, relativeNoLoadCurrent, nominalPhaseShiftOneToTwo, nominalPhaseShiftTwoToThree, nominalPhaseShiftThreeToOne,
                name);
        }

        public void AddLoad(int nodeId, Complex power)
        {
            _singlePhasePowerNet.AddLoad(nodeId, power/3);
        }

        public void AddImpedanceLoad(int nodeId, Complex impedance)
        {
            _singlePhasePowerNet.AddImpedanceLoad(nodeId, impedance);
        }

        public IReadOnlyDictionary<long, NodeResult> CalculateNodeVoltages()
        {
            var nodeResults = _singlePhasePowerNet.CalculateNodeVoltages();
            var nodeResultsUnscaled = new Dictionary<long, NodeResult>();

            foreach (var nodeResultWithId in nodeResults)
            {
                var nodeResult = nodeResultWithId.Value;
                var nodeResultUnscaled = new NodeResult(nodeResult.Voltage*Math.Sqrt(3), nodeResult.Power*3);
                nodeResultsUnscaled.Add(nodeResultWithId.Key, nodeResultUnscaled);
            }

            return nodeResultsUnscaled;
        }

        public void CalculateAdmittanceMatrix(out AdmittanceMatrix matrix, out IReadOnlyList<string> nodeNames, out double powerBase)
        {
            powerBase = _singlePhasePowerNet.DeterminePowerScaling();
            _singlePhasePowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, powerBase);
        }

        public Angle GetSlackPhaseShift()
        {
            return _singlePhasePowerNet.GetSlackPhaseShift();
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> GetNominalPhaseShiftPerNode()
        {
            return _singlePhasePowerNet.GetNominalPhaseShiftPerNode();
        }
    }
}
