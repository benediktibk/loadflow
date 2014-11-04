using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Numerics;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class SlackGenerator : INetElement
    {
        public SlackGenerator(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var machineType = record.Parse<int>("Flag_Machine");
            var nominalVoltage = record.Parse<double>("Un") * 1000;
            var loadFlowType = record.Parse<int>("Flag_Lf");
            var relativeSynchronousReactance = record.Parse<double>("xi") / 100;
            double voltageMagnitude;

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (Math.Abs(nominalVoltage - nodes[NodeId].NominalVoltage) > 0.000001)
                throw new InvalidDataException(
                    "the nominal voltage of a generator does not match the nominal voltage of the connected node");

            if (relativeSynchronousReactance != 0)
                throw new NotSupportedException("an internal reactance for a slack generator is not supported");

            switch (loadFlowType)
            {
                case 3:
                    voltageMagnitude = nominalVoltage * record.Parse<double>("u") / 100;
                    break;
                case 5:
                    voltageMagnitude = record.Parse<double>("Ug") * 1000;
                    break;
                default:
                    throw new NotSupportedException("the selected load flow type of a generator is not supported");
            }

            var delta = record.Parse<double>("delta")*Math.PI/180;
            Voltage = Complex.FromPolarCoordinates(voltageMagnitude, delta);
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public Complex Voltage { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddFeedIn(NodeId, Voltage, 0, 0, 0);
        }
    }
}
