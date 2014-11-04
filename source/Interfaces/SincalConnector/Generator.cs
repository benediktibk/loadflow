using System;
using System.Collections.Generic;
using System.Data.OleDb;
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
            var nominalVoltage = record.Parse<double>("Un") * 1000;
            var loadFlowType = record.Parse<int>("Flag_Lf");
            var relativeSynchronousReactance = record.Parse<double>("xi")/100;
            var powerFactor = record.Parse<double>("fP");

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (Math.Abs(nominalVoltage - nodes[NodeId].NominalVoltage) > 0.000001)
                throw new InvalidDataException(
                    "the nominal voltage of a generator does not match the nominal voltage of the connected node");

            if (relativeSynchronousReactance != 0)
                throw new NotSupportedException("internal reactances for a generator are not supported");

            if (powerFactor != 1)
                throw new NotSupportedException("a power factor (fP) different than 1 is not supported");

            switch (loadFlowType)
            {
                case 6: case 11:
                    VoltageMagnitude = nominalVoltage * record.Parse<double>("u") / 100;
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

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddGenerator(NodeId, VoltageMagnitude, RealPower);
        }

        public static OleDbCommand CreateCommandToFetchAllGenerators()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Machine,Un,Flag_Lf,P,u,Ug,xi,fP FROM SynchronousMachine WHERE Flag_Lf = 6 OR Flag_Lf = 7 OR Flag_Lf = 11 OR Flag_Lf = 12;");
        }
    }
}
