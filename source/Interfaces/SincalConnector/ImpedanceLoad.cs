using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.ThreePhase;
using MathNet.Numerics;
using Misc;

namespace SincalConnector
{
    public class ImpedanceLoad : INetElement
    {
        public ImpedanceLoad(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var modelType = record.Parse<int>("Flag_Lf");
            var realPower = record.Parse<double>("P")*1e6;
            var reactivePower = record.Parse<double>("Q")*1e6;
            double voltage;

            switch (modelType)
            {
                case 1:
                    var nominalVoltage = nodes[NodeId].NominalVoltage;
                    voltage = record.Parse<double>("u")/100*nominalVoltage;
                    break;
                case 2:
                    voltage = record.Parse<double>("Ul")*1000;
                    break;
                default:
                    throw new NotSupportedException("model type of impedance load is not supported");
            }

            var powerPerLine = new Complex(realPower, reactivePower);
            var current = Complex.Conjugate(powerPerLine/voltage);
            Impedance = voltage/current;
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public Complex Impedance { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            powerNet.AddImpedanceLoad(NodeId, Impedance);
        }

        public void FixNodeResult(IDictionary<int, NodeResult> nodeResults)
        {
            var nodeResult = nodeResults[NodeId];
            var loadByImpedance = nodeResult.Voltage * (nodeResult.Voltage / Impedance).Conjugate();
            var nodeResultModified = new NodeResult(NodeId, nodeResult.Voltage, nodeResult.Power - loadByImpedance);
            nodeResults[NodeId] = nodeResultModified;
        }
    }
}
