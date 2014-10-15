using System;
using System.Data.OleDb;
using System.Numerics;
using DatabaseHelper;

namespace SincalConnector
{
    public class Node : IReadOnlyNode
    {
        #region variables

        private readonly OleDbConnection _databaseConnection;

        #endregion

        #region constructor

        public Node(ISafeDatabaseRecord record, OleDbConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
            var nameFull = record.Parse<string>("Name");
            var nameLength = nameFull.Length;

            while (nameLength > 0 && nameFull[nameLength - 1] == ' ')
                --nameLength;

            Id = record.Parse<int>("Id");
            Name = nameFull.Substring(0, nameLength);
            NominalVoltage = record.Parse<double>("Un")*1000;
            Voltage = new Complex();
        }

        #endregion

        #region IReadOnlyNode

        public int Id { get; private set; }

        public string Name { get; private set; }

        public double NominalVoltage { get; private set; }

        public Complex Voltage { get; private set; }

        public Complex Load { get; private set; }

        #endregion

        #region public functions

        public void SetResult(Complex voltage, Complex load)
        {
            Voltage = voltage;
            Load = load;
        }

        public OleDbCommand CreateCommandToAddResult(double rotationOffset)
        {
            var command = new OleDbCommand("INSERT INTO LFNodeResults (Flag_Result,Node_ID,P,Q,S,U,U_Un,phi,phi_rot) VALUES (0,@Id,@P,@Q,@S,@U,@URelative,@phi,@phi_rot)");
            command.Parameters.AddWithValue("Id", Id);
            command.Parameters.AddWithValue("P", Load.Real * 1e-6);
            command.Parameters.AddWithValue("Q", Load.Imaginary * 1e-6);
            command.Parameters.AddWithValue("S", Load.Magnitude * 1e-6);
            command.Parameters.AddWithValue("U", Voltage.Magnitude * 1e-6);
            command.Parameters.AddWithValue("URelative", Voltage.Magnitude / NominalVoltage * 100);
            command.Parameters.AddWithValue("phi", (Voltage.Phase - rotationOffset) * 180 / Math.PI);
            command.Parameters.AddWithValue("phi_rot", Voltage.Phase * 180 / Math.PI);
            return command;
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Node.Node_ID AS Id,Node.Name AS Name,VoltageLevel.Un AS Un FROM Node INNER JOIN VoltageLevel ON VoltageLevel.VoltLevel_ID = Node.VoltLevel_ID;");
        }

        public static OleDbCommand CreateCommandToDeleteAllResults()
        {
            return new OleDbCommand("DELETE FROM LFNodeResult;");
        }

        #endregion
    }
}
