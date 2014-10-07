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

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Node.Node_ID AS Id,Node.Name AS Name,VoltageLevel.Un AS Un FROM Node INNER JOIN VoltageLevel ON VoltageLevel.VoltLevel_ID = Node.VoltLevel_ID;");
        }

        #endregion
    }
}
