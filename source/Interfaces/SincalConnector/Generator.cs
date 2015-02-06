using System;
using System.Collections.Generic;
using System.IO;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class Generator : INetElement
    {
        public Generator(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var machineType = record.Parse<int>("Flag_Machine");
            var loadFlowType = record.Parse<int>("Flag_Lf");
            var relativeSynchronousReactance = record.Parse<double>("xi")/100;
            var powerFactor = record.Parse<double>("fP");

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (relativeSynchronousReactance != 0)
                throw new NotSupportedException("internal reactances for a generator are not supported");

            if (powerFactor != 1)
                throw new NotSupportedException("a power factor (fP) different than 1 is not supported");

            switch (loadFlowType)
            {
                case 6: case 11:
                    VoltageMagnitude = nodes[NodeId].NominalVoltage * record.Parse<double>("u") / 100;
                    break;
                case 7: case 12:
                    VoltageMagnitude = record.Parse<double>("Ug") * 1000;
                    break;
                default:
                    throw new NotSupportedException("the selected load flow type of a generator is not supported");
            }

            RealPower = record.Parse<double>("P")*1e6;
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public double VoltageMagnitude { get; private set; }

        public double RealPower { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            powerNet.AddGenerator(NodeId, VoltageMagnitude, RealPower*powerFactor);
        }
    }
}
