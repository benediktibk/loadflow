using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class FeedIn : INetElement
    {
        public FeedIn(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            var admittanceType = record.Parse<int>("Flag_Typ");

            if (admittanceType != 2)
                throw new NotSupportedException("a feed-in must be specified by R/X and Sk");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var nominalVoltage = nodes[NodeId].NominalVoltage;
            var voltageAngle = record.Parse<double>("delta")*Math.PI/180;
            var voltageType = record.Parse<int>("Flag_Lf");
            var realToImaginary = record.Parse<double>("R_X");
            var shortCircuitPower = record.Parse<double>("Sk2")*1e6;
            var internalReactance = record.Parse<double>("xi") / 100;
            double voltageMagnitude;

            switch (voltageType)
            {
                case 3:
                    var voltageMagnitudeRelative = record.Parse<double>("u")/100;
                    voltageMagnitude = voltageMagnitudeRelative*nominalVoltage;
                    break;
                case 6:
                    voltageMagnitude = record.Parse<double>("Ug")*1000;
                    break;
                default:
                    throw new NotSupportedException("only the voltage types '|uq| und delta' and '|Uq| und delta' are supported for a feed-in");
            }

            Voltage = Complex.FromPolarCoordinates(voltageMagnitude, voltageAngle);
            InternalImpedance = internalReactance*nominalVoltage*nominalVoltage/shortCircuitPower*
                                new Complex(realToImaginary, 1);
        }

        public Complex Voltage { get; private set; }

        public Complex InternalImpedance { get; private set; }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddFeedIn(NodeId, Voltage, InternalImpedance);
        }
    }
}
