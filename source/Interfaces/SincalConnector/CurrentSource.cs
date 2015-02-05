using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Calculation.ThreePhase;
using MathNet.Numerics;
using Misc;

namespace SincalConnector
{
    public class CurrentSource : INetElement
    {
        public CurrentSource(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var machineType = record.Parse<int>("Flag_Machine");
            var nominalVoltage = record.Parse<double>("Un") * 1000;
            var nominalPower = record.Parse<double>("Sn")*1e6;
            var relativeSynchronousReactance = record.Parse<double>("xi")/100;
            var realToImaginaryResistance = record.Parse<double>("R_X");
            var powerFactor = record.Parse<double>("fP");
            var realPower = record.Parse<double>("P");
            var imaginaryPower = record.Parse<double>("Q");

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (Math.Abs(nominalVoltage - nodes[NodeId].NominalVoltage) > 0.000001)
                throw new InvalidDataException(
                    "the nominal voltage of a generator does not match the nominal voltage of the connected node");

            if (powerFactor != 1)
                throw new NotSupportedException("a power factor (fP) different than 1 is not supported");

            var power = new Complex(realPower, imaginaryPower);
            Current = (power/(Math.Sqrt(3)*nominalVoltage)).Conjugate();
            InternalImpedance = relativeSynchronousReactance*new Complex(realToImaginaryResistance, 1)*nominalVoltage*
                                nominalVoltage/nominalPower;
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public Complex Current { get; private set; }

        public Complex InternalImpedance { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            powerNet.AddCurrentSource(NodeId, Current, InternalImpedance);
        }
    }
}
