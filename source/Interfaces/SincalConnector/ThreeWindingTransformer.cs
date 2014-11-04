using System;
using System.Collections.Generic;
using System.Data.OleDb;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class ThreeWindingTransformer : INetElement
    {
        public ThreeWindingTransformer(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NominalPowerOneToTwo = record.Parse<double>("Sn12") * 1e6;
            NominalPowerTwoToThree = record.Parse<double>("Sn23") * 1e6;
            NominalPowerThreeToOne = record.Parse<double>("Sn31") * 1e6;
            RelativeShortCircuitVoltageOneToTwo = record.Parse<double>("uk12") / 100;
            RelativeShortCircuitVoltageTwoToThree = record.Parse<double>("uk23") / 100;
            RelativeShortCircuitVoltageThreeToOne = record.Parse<double>("uk31") / 100;
            IronLosses = record.Parse<double>("Vfe") * 1000;
            RelativeNoLoadCurrent = record.Parse<double>("i0") / 100;
            var realRelativeShortCircuitVoltageOneToTwo = record.Parse<double>("ur12") / 100;
            var realRelativeShortCircuitVoltageTwoToThree = record.Parse<double>("ur23") / 100;
            var realRelativeShortCircuitVoltageThreeToOne = record.Parse<double>("ur31") / 100;
            CopperLossesOneToTwo = NominalPowerOneToTwo * realRelativeShortCircuitVoltageOneToTwo;
            CopperLossesTwoToThree = NominalPowerTwoToThree * realRelativeShortCircuitVoltageTwoToThree;
            CopperLossesThreeToOne = NominalPowerThreeToOne * realRelativeShortCircuitVoltageThreeToOne;
            var controlStageOne = record.Parse<double>("roh1");
            var controlStageTwo = record.Parse<double>("roh2");
            var controlStageThree = record.Parse<double>("roh3");
            var additionalPhaseShiftOneToTwo = record.Parse<double>("AddRotate1");
            var additionalPhaseShiftTwoToThree = record.Parse<double>("AddRotate2");
            var additionalPhaseShiftThreeToOne = record.Parse<double>("AddRotate3");

            if (controlStageOne != 0 || controlStageTwo != 0 || controlStageThree != 0)
                throw new NotSupportedException("control stages are not supported");

            if (additionalPhaseShiftOneToTwo != 0 || additionalPhaseShiftTwoToThree != 0 || additionalPhaseShiftThreeToOne != 0)
                throw new NotSupportedException("additional phase shifts at the transformer are not supported");

            var connectionSymbolOneToTwo = record.Parse<int>("VecGrp1");
            var connectionSymbolTwoToThree = record.Parse<int>("VecGrp2");
            var connectionSymbolThreeToOne = record.Parse<int>("VecGrp3");
            var phaseShiftFactorOneToTwo = MapConnectionSymbolToPhaseShiftFactor(connectionSymbolOneToTwo);
            var phaseShiftFactorTwoToThree = MapConnectionSymbolToPhaseShiftFactor(connectionSymbolTwoToThree);
            var phaseShiftFactorThreeToOne = MapConnectionSymbolToPhaseShiftFactor(connectionSymbolThreeToOne);
            PhaseShiftOneToTwo = Angle.FromDegree(phaseShiftFactorOneToTwo * 30);
            PhaseShiftTwoToThree = Angle.FromDegree(phaseShiftFactorTwoToThree * 30);
            PhaseShiftThreeToOne = Angle.FromDegree(phaseShiftFactorThreeToOne * 30);

            var nominalVoltageOne = record.Parse<double>("Un1")*1000;
            var nominalVoltageTwo = record.Parse<double>("Un2") * 1000;
            var nominalVoltageThree = record.Parse<double>("Un3") * 1000;
            var nominalVoltages = new List<double>() {nominalVoltageOne, nominalVoltageTwo, nominalVoltageThree};
            var connectedNodeIds = new List<int>(nodeIdsByElementIds.Get(Id));

            if (connectedNodeIds.Count != 3)
                throw new NotSupportedException("a three winding transformer must be connected to exactly three nodes");

            NodeOneId = connectedNodeIds[0];
            NodeTwoId = connectedNodeIds[1];
            NodeThreeId = connectedNodeIds[2];

            for (var i = 0; i < 3; ++i)
            {
                var nominalVoltage = nominalVoltages[i];
                var node = nodes[connectedNodeIds[i]];

                if (Math.Abs(node.NominalVoltage - nominalVoltage) > 0.00001)
                    throw new NotSupportedException("only nominal ratios are supported");
            }
        }

        public int Id { get; private set; }

        public int NodeOneId { get; private set; }

        public int NodeTwoId { get; private set; }

        public int NodeThreeId { get; private set; }

        public double NominalPowerOneToTwo { get; private set; }

        public double NominalPowerTwoToThree { get; private set; }

        public double NominalPowerThreeToOne { get; private set; }

        public double RelativeShortCircuitVoltageOneToTwo { get; private set; }

        public double RelativeShortCircuitVoltageTwoToThree { get; private set; }

        public double RelativeShortCircuitVoltageThreeToOne { get; private set; }

        public double CopperLossesOneToTwo { get; private set; }

        public double CopperLossesTwoToThree { get; private set; }

        public double CopperLossesThreeToOne { get; private set; }

        public double IronLosses { get; private set; }

        public double RelativeNoLoadCurrent { get; private set; }

        public Angle PhaseShiftOneToTwo { get; private set; }

        public Angle PhaseShiftTwoToThree { get; private set; }

        public Angle PhaseShiftThreeToOne { get; private set; }

        public static OleDbCommand CreateCommandToFetchAllThreeWindingTransformers()
        {
            return new OleDbCommand("SELECT Element_ID,Sn12,Sn23,Sn31,uk12,uk23,uk31,ur12,ur23,ur31,Vfe,i0,VecGrp1,VecGrp2,VecGrp3,roh1,roh2,roh3,Un1,Un2,Un3,AddRotate1,AddRotate2,AddRotate3 FROM ThreeWindingTransformer;");
        }

        public static int MapConnectionSymbolToPhaseShiftFactor(int connectionSymbol)
        {
            switch (connectionSymbol)
            {
                case 1:
                case 2:
                    return 0;
                case 3:
                case 4:
                    return 6;
                case 5:
                case 9:
                case 10:
                    return 1;
                case 6:
                case 11:
                case 12:
                    return 5;
                case 7:
                case 13:
                case 14:
                    return 7;
                case 8:
                case 15:
                case 16:
                    return 11;
                default:
                    throw new NotSupportedException("connection type of transformer is not supported");
            }
        }

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddThreeWindingTransformer(NodeOneId, NodeTwoId, NodeThreeId, NominalPowerOneToTwo,
                NominalPowerTwoToThree, NominalPowerThreeToOne, RelativeShortCircuitVoltageOneToTwo,
                RelativeShortCircuitVoltageTwoToThree, RelativeShortCircuitVoltageThreeToOne, CopperLossesOneToTwo,
                CopperLossesTwoToThree, CopperLossesThreeToOne, IronLosses, RelativeNoLoadCurrent, PhaseShiftOneToTwo,
                PhaseShiftTwoToThree, PhaseShiftThreeToOne, "");
        }
    }
}
