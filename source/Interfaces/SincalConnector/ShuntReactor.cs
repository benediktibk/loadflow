using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Calculation.ThreePhase;
using MathNet.Numerics;
using Misc;

namespace SincalConnector
{
    public class ShuntReactor : INetElement
    {
        public ShuntReactor(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var nominalVoltage = record.Parse<double>("Un")*1e3;
            var ironLosses = record.Parse<double>("Vfe") * 1e3;
            var copperLosses = record.Parse<double>("Vcu") * 1e3;
            var nominalPower = record.Parse<double>("Sn")*1e6;
            var totalPower = new Complex(ironLosses + copperLosses, (-1)*nominalPower);
            ImpedanceValid = totalPower.MagnitudeSquared() > 0;

            if (ImpedanceValid)
                Impedance = nominalVoltage*nominalVoltage/totalPower;
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public Complex Impedance { get; private set; }

        public bool ImpedanceValid { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            if (ImpedanceValid)
                powerNet.AddImpedanceLoad(NodeId, Impedance);
        }

        public void FixNodeResult(IDictionary<int, NodeResult> nodeResults)
        {
            if (!ImpedanceValid)
                return;

            var nodeResult = nodeResults[NodeId];
            var loadByImpedance = nodeResult.Voltage * (nodeResult.Voltage / Impedance).Conjugate();
            var nodeResultModified = new NodeResult(NodeId, nodeResult.Voltage, nodeResult.Power - loadByImpedance);
            nodeResults[NodeId] = nodeResultModified;
        }
    }
}
