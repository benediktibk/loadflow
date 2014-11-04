using System.Data.OleDb;
using System.Numerics;
using Misc;

namespace SincalConnector
{
    public class NodeResult
    {
        public NodeResult(ISafeDatabaseRecord record)
        {
            var voltageMagnitude = record.Parse<double>("U")*1e3;
            var voltageAngle = Angle.FromDegree(record.Parse<double>("phi_rot"));
            var realPower = record.Parse<double>("P") * 1e6;
            var reactivePower = record.Parse<double>("Q") * 1e6;
            NodeId = record.Parse<int>("Node_ID");
            Voltage = Complex.FromPolarCoordinates(voltageMagnitude, voltageAngle.Radiant);
            Power = new Complex(realPower, reactivePower);
        }

        public int NodeId { get; private set; }

        public Complex Voltage { get; private set; }

        public Complex Power { get; private set; }

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Node_ID,U,phi_rot,P,Q FROM LFNodeResult;");
        }

        public static OleDbCommand CreateCommandToDeleteAll()
        {
            return new OleDbCommand("DELETE FROM LFNodeResult;");
        }
    }
}
