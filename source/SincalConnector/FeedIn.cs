using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Numerics;
using Calculation.ThreePhase;
using DatabaseHelper;

namespace SincalConnector
{
    public class FeedIn : INetElement
    {
        #region constructor

        public FeedIn(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            var admittanceType = record.Parse<int>("Flag_Typ");

            if (admittanceType != 2)
                throw new NotSupportedException("a feed-in must be specified by R/X and Sk");

            var internalReactance = record.Parse<double>("xi");

            if (internalReactance != 0)
                throw new NotSupportedException("an internal reactance for a feed-in is not supported");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var voltageAngle = record.Parse<double>("delta")*Math.PI/180;
            var voltageType = record.Parse<int>("Flag_Lf");
            double voltageMagnitude;

            switch (voltageType)
            {
                case 3:
                    var voltageMagnitudeRelative = record.Parse<double>("u")/100;
                    var nominalVoltage = nodes[NodeId].NominalVoltage;
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

        #endregion

        #region properties

        public Complex Voltage { get; private set; }
        public int Id { get; private set; }
        public int NodeId { get; private set; }

        #endregion

        #region public functions

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddFeedIn(NodeId, Voltage, 0, 0, 0);
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Typ,Flag_Lf,delta,u,Ug,xi FROM Infeeder;");
        }

        #endregion
    }
}
