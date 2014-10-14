using System.Data.OleDb;
using System.Numerics;
using DatabaseHelper;

namespace SincalConnector
{
    public class ImpedanceLoad
    {
        #region constructor

        public ImpedanceLoad(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeId { get; private set; }
        public Complex Impedance { get; private set; }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Lf,P,Q FROM Load WHERE Flag_LoadType = 1 AND Flag_Load = 1;");
        }

        #endregion
    }
}
