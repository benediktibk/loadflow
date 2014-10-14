using System;
using System.Data.OleDb;
using System.IO;
using System.Numerics;
using DatabaseHelper;

namespace SincalConnector
{
    public class Load
    {
        #region constructor

        public Load(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            var loadTypeOne = record.Parse<int>("Flag_Load");
            var loadTypeTwo = record.Parse<int>("Flag_LoadType");
            var modelType = record.Parse<int>("Flag_Lf");

            if (loadTypeOne != 1 || loadTypeTwo != 2 || modelType != 1)
                throw new NotSupportedException("not supported load type");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var p = record.Parse<double>("P") * 1e6;
            var q = record.Parse<double>("Q") * 1e6;
            LoadValue = new Complex(p, q);
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeId { get; private set; }
        public Complex LoadValue { get; private set; }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Load,Flag_LoadType,Flag_Lf,P,Q FROM Load;");
        }

        #endregion
    }
}
