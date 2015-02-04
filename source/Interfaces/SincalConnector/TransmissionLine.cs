using System;
using System.Collections.Generic;
using System.IO;
using Calculation.SinglePhase.MultipleVoltageLevels;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class TransmissionLine : INetElement
    {
        public TransmissionLine(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds, IReadOnlyDictionary<int, IReadOnlyNode> nodesByIds, double frequency)
        {
            Id = record.Parse<int>("Element_ID");
            var lineType = record.Parse<int>("Flag_LineTyp");

            switch (lineType)
            {
                case 1:
                case 2:
                    Length = record.Parse<double>("l") * 1000;
                    break;
                case 3:
                    Length = 0;
                    break;
                default:
                    throw new NotSupportedException("the selected transmission line type is not supported");
            }

            var nodes = nodeIdsByElementIds.Get(Id);

            if (nodes.Count > 2)
                throw new InvalidDataException("a transmission line can have only two connected nodes");

            NotConnected = nodes.Count < 1;

            if (NotConnected)
                return;

            NodeOneId = nodes[0];
            var nodeOneNominalVoltage = nodesByIds[NodeOneId].NominalVoltage;

            FullyConnected = nodes.Count == 2;

            if (FullyConnected)
            {
                NodeTwoId = nodes[1];

                var nodeTwoNominalVoltage = nodesByIds[NodeTwoId].NominalVoltage;

                if (Math.Abs(nodeOneNominalVoltage - nodeTwoNominalVoltage) > 0.000001)
                    throw new InvalidDataException("the nominal voltages at a transmission line do not match");
            }

            SeriesResistancePerUnitLength = record.Parse<double>("r") / 1000;
            SeriesInductancePerUnitLength = record.Parse<double>("x") / (2 * Math.PI * frequency * 1000);
            var shuntLosses = record.Parse<double>("va");
            var nominalVoltage = record.Parse<double>("Un")*1000;

            if (nominalVoltage == 0)
                nominalVoltage = nodeOneNominalVoltage;

            ShuntConductancePerUnitLength = shuntLosses / (nominalVoltage * nominalVoltage);
            ShuntCapacityPerUnitLength = record.Parse<double>("c") / 1e12;
            TransmissionEquationModel = record.Parse<int>("Flag_Ll") == 1;

            if (Math.Abs(frequency - record.Parse<double>("fn")) > 0.00001)
                throw new NotSupportedException("the frequency of a transmission line does not match the net frequency");

            var parallelSystems = record.Parse<double>("ParSys");

            if (parallelSystems != 1)
                throw new NotSupportedException("parallel transmission lines are not supported");
        }

        public int Id { get; private set; }

        public int NodeOneId { get; private set; }

        public int NodeTwoId { get; private set; }

        public bool FullyConnected { get; private set; }

        public bool NotConnected { get; private set; }

        public double SeriesResistancePerUnitLength { get; private set; }

        public double SeriesInductancePerUnitLength { get; private set; }

        public double ShuntConductancePerUnitLength { get; private set; }

        public double ShuntCapacityPerUnitLength { get; private set; }

        public double Length { get; private set; }

        public bool TransmissionEquationModel { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            if (NotConnected)
                return;

            if (FullyConnected)
                powerNet.AddTransmissionLine(NodeOneId, NodeTwoId, SeriesResistancePerUnitLength,
                    SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength, Length,
                    TransmissionEquationModel);
            else
            {
                var data = new TransmissionLineData(SeriesResistancePerUnitLength, SeriesInductancePerUnitLength,
                    ShuntCapacityPerUnitLength, ShuntConductancePerUnitLength, Length, powerNet.Frequency,
                    TransmissionEquationModel);

                if (!data.NeedsGroundNode) 
                    return;

                powerNet.AddImpedanceLoad(NodeOneId, 1 / data.ShuntAdmittance);
                powerNet.AddImpedanceLoad(NodeOneId, data.LengthImpedance + 1 / data.ShuntAdmittance);
            }
        }
    }
}
