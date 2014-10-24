using System;
using System.Data.OleDb;
using System.Numerics;
using Calculation.ThreePhase;
using DatabaseHelper;

namespace SincalConnector
{
    public class Load : INetElement
    {
        #region constructor

        public Load(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            var modelType = record.Parse<int>("Flag_Lf");

            if (modelType != 1 && modelType != 2)
                throw new NotSupportedException("not supported load model");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var p = record.Parse<double>("P") * 1e6;
            var q = record.Parse<double>("Q") * 1e6;
            LoadValue = new Complex(p, q)*(-1);
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeId { get; private set; }
        public Complex LoadValue { get; private set; }

        #endregion

        #region public functions

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddLoad(NodeId, LoadValue);
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Lf,P,Q FROM Load WHERE (Flag_LoadType = 2 OR Flag_LoadType = 4) AND Flag_Load = 1;");
        }

        #endregion
    }
}
