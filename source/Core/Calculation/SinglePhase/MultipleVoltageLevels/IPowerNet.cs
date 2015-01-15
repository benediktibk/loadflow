using System.Collections.Generic;
using System.Numerics;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNet
    {
        IReadOnlyNodeGraph NodeGraph { get; }
        Angle SlackPhaseShift { get; }
        IReadOnlyDictionary<IExternalReadOnlyNode, Angle> NominalPhaseShiftPerNode { get; }
        void AddNode(int id, double nominalVoltage, string name);
        void AddTransmissionLine(int sourceNodeId, int targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel);
        void AddGenerator(int nodeId, double voltageMagnitude, double realPower);
        void AddFeedIn(int nodeId, Complex voltage, Complex internalImpedance);
        void AddTwoWindingTransformer(int upperSideNodeId, int lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name);
        void AddThreeWindingTransformer(int nodeOneId, int nodeTwoId, int nodeThreeId, double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne, double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree, double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo, double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses, double relativeNoLoadCurrent, Angle nominalPhaseShiftOne, Angle nominalPhaseShiftTwo, Angle nominalPhaseShiftThree, string name);
        void AddLoad(int nodeId, Complex power);
        void AddImpedanceLoad(int nodeId, Complex impedance);
    }
}