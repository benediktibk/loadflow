using System;
using System.Data.OleDb;
using System.Numerics;
using DatabaseHelper;

namespace SincalConnector
{
    public class NodeResult
    {
        #region constructor

        public NodeResult(ISafeDatabaseRecord record)
        {
            var voltageMagnitude = record.Parse<double>("U")*1e3;
            var voltageAngle = record.Parse<double>("phi_rot")*Math.PI/180;
            var realPower = record.Parse<double>("P") * 1e6;
            var reactivePower = record.Parse<double>("Q") * 1e6;
            NodeId = record.Parse<int>("Node_ID");
            Voltage = Complex.FromPolarCoordinates(voltageMagnitude, voltageAngle);
            Power = new Complex(realPower, reactivePower);
        }

        #endregion

        #region properties

        public int NodeId { get; private set; }
        public Complex Voltage { get; private set; }
        public Complex Power { get; private set; }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Node_ID,U,phi_rot,P,Q FROM LFNodeResult;");
        }

        public static OleDbCommand CreateCommandToDeleteAll()
        {
            return new OleDbCommand("DELETE FROM LFNodeResult;");
        }

        #endregion
    }
}
