using System.Collections.Generic;
using System.Numerics;
using Misc;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public interface IPowerNet
    {
        Angle SlackPhaseShift { get; }
        IReadOnlyDictionary<IExternalReadOnlyNode, Angle> CalculateNominalPhaseShiftPerNode();
        void AddNode(int id, double nominalVoltage, string name);
        void AddTransmissionLine(long sourceNodeId, long targetNodeId, double seriesResistancePerUnitLength, double seriesInductancePerUnitLength, double shuntConductancePerUnitLength, double shuntCapacityPerUnitLength, double length, bool transmissionEquationModel);
        void AddGenerator(long nodeId, double voltageMagnitude, double realPower);
        void AddFeedIn(long nodeId, Complex voltage, double shortCircuitPower, double c, double realToImaginary);
        void AddTwoWindingTransformer(long upperSideNodeId, long lowerSideNodeId, double nominalPower, double relativeShortCircuitVoltage, double copperLosses, double ironLosses, double relativeNoLoadCurrent, double ratio, Angle nominalPhaseShift, string name);

        void AddThreeWindingTransformer(long nodeOneId, long nodeTwoId, long nodeThreeId, double nominalPowerOneToTwo, double nominalPowerTwoToThree, double nominalPowerThreeToOne,
            double relativeShortCircuitVoltageOneToTwo, double relativeShortCircuitVoltageTwoToThree, double relativeShortCircuitVoltageThreeToOne, double copperLossesOneToTwo, 
            double copperLossesTwoToThree, double copperLossesThreeToOne, double ironLosses, double relativeNoLoadCurrent, Angle nominalPhaseShiftOneToTwo, 
            Angle nominalPhaseShiftTwoToThree, Angle nominalPhaseShiftThreeToOne, string name);

        void AddLoad(long nodeId, Complex power);
        void AddImpedanceLoad(long nodeId, Complex impedance);
    }
}