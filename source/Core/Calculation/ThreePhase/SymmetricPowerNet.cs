using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Misc;
using AdmittanceMatrix = Calculation.SinglePhase.MultipleVoltageLevels.AdmittanceMatrix;
using IPowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.IPowerNetComputable;
using PowerNetComputable = Calculation.SinglePhase.MultipleVoltageLevels.PowerNetComputable;

namespace Calculation.ThreePhase
{
    public class SymmetricPowerNet
    {
        private readonly IPowerNetComputable _singlePhasePowerNet;

        public SymmetricPowerNet(IPowerNetComputable singlePhasePowerNet)
        {
            _singlePhasePowerNet = singlePhasePowerNet;
        }

        public Angle SlackPhaseShift
        {
            get { return _singlePhasePowerNet.SlackPhaseShift; }
        }

        public IReadOnlyNodeGraph NodeGraph
        {
            get { return _singlePhasePowerNet.NodeGraph; }
        }

        public double Frequency
        {
            get { return _singlePhasePowerNet.Frequency; }
        }

        public void AddNode(int id, double nominalVoltage, string name)
        {
            _singlePhasePowerNet.AddNode(id, nominalVoltage/Math.Sqrt(3), name);
        }

        public void AddTransmissionLine(int sourceNodeId, int targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel)
        {
            _singlePhasePowerNet.AddTransmissionLine(sourceNodeId, targetNodeId, seriesResistancePerUnitLength, seriesInductancePerUnitLength, shuntConductancePerUnitLength, shuntCapacityPerUnitLength, length, transmissionEquationModel);
        }

        public void AddGenerator(int nodeId, double voltageMagnitude, double realPower)
        {
            _singlePhasePowerNet.AddGenerator(nodeId, voltageMagnitude/Math.Sqrt(3), realPower/3);
        }

        public void AddFeedIn(int nodeId, Complex voltage, Complex internalImpedance)
        {
            _singlePhasePowerNet.AddFeedIn(nodeId, voltage/Math.Sqrt(3), internalImpedance);
        }

        public void AddTwoWindingTransformer(int upperSideNodeId, int lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name)
        {
            _singlePhasePowerNet.AddTwoWindingTransformer(upperSideNodeId, lowerSideNodeId, nominalPower/3,
                relativeShortCircuitVoltage, copperLosses/3, ironLosses/3, relativeNoLoadCurrent, ratio, nominalPhaseShift, name);
        }

        public void AddThreeWindingTransformer(int nodeOneId, int nodeTwoId, int nodeThreeId, double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne, double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree, double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses, double relativeNoLoadCurrent, Angle nominalPhaseShiftOne, Angle nominalPhaseShiftTwo, Angle nominalPhaseShiftThree, string name)
        {
            _singlePhasePowerNet.AddThreeWindingTransformer(nodeOneId, nodeTwoId, nodeThreeId, nominalPowerOneToTwo/3, nominalPowerTwoToThree/3,
                nominalPowerThreeToOne/3, relativeShortCircuitVoltageOneToTwo, relativeShortCircuitVoltageTwoToThree,
                relativeShortCircuitVoltageThreeToOne, copperLossesOneToTwo/3, copperLossesTwoToThree/3, copperLossesThreeToOne/3,
                ironLosses/3, relativeNoLoadCurrent, nominalPhaseShiftOne, nominalPhaseShiftTwo, nominalPhaseShiftThree,
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

        public IReadOnlyDictionary<int, NodeResult> CalculateNodeVoltages(out double relativePowerError)
        {
            var nodeResults = _singlePhasePowerNet.CalculateNodeResults(out relativePowerError);
            var nodeResultsUnscaled = new Dictionary<int, NodeResult>();

            if (nodeResults == null)
                return null;

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
            _singlePhasePowerNet.CalculateAdmittanceMatrix(out matrix, out nodeNames, out powerBase);
        }

        public IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CalculateNominalPhaseShiftPerNode()
        {
            return _singlePhasePowerNet.NominalPhaseShiftPerNode;
        }

        public static SymmetricPowerNet Create(INodeVoltageCalculator nodeVoltageCalculator, double frequency)
        {
            var nodeGraph = new NodeGraph();
            var singleVoltagePowerNetFactory = new PowerNetFactory(nodeVoltageCalculator);
            var singlePhasePowerNet = new PowerNetComputable(frequency, singleVoltagePowerNetFactory, nodeGraph);
            var symmetricPowerNet = new SymmetricPowerNet(singlePhasePowerNet);
            return symmetricPowerNet;
        }
    }
}
