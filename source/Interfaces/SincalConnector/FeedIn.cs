﻿using System;
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
            var internalReactance = record.Parse<double>("xi") / 100;

            if (admittanceType != 2)
                throw new NotSupportedException("a feed-in must be specified by R/X and Sk");

            if (internalReactance != 0)
                throw new NotSupportedException("internal impedances for feed ins are not supported");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var nominalVoltage = nodes[NodeId].NominalVoltage;
            var voltageAngle = record.Parse<double>("delta")*Math.PI/180;
            var voltageType = record.Parse<int>("Flag_Lf");
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
        }

        public Complex Voltage { get; private set; }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            powerNet.AddFeedIn(NodeId, Voltage, new Complex());
        }

        public void FixNodeResult(IDictionary<int, NodeResult> nodeResults)
        { }
    }
}
