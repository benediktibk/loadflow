using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using DatabaseHelper;

namespace SincalConnector
{
    public class Generator
    {
        #region constructor

        public Generator(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var machineType = record.Parse<int>("Flag_Machine");
            var nominalVoltage = record.Parse<double>("Un") * 1000;
            var loadFlowType = record.Parse<int>("Flag_Lf");

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (Math.Abs(nominalVoltage - nodes[NodeId].NominalVoltage) > 0.000001)
                throw new InvalidDataException(
                    "the nominal voltage of a generator does not match the nominal voltage of the connected node");

            switch (loadFlowType)
            {
                case 11:
                    VoltageMagnitude = nominalVoltage*record.Parse<double>("u")/100;
                    break;
                case 12:
                    VoltageMagnitude = record.Parse<double>("Ug")*1000;
                    break;
                default:
                    throw new NotSupportedException("the selected load flow type of a generator is not supported");
            }

            RealPower = record.Parse<double>("P")*1e6;
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeId { get; private set; }
        public double VoltageMagnitude { get; private set; }
        public double RealPower { get; private set; }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Machine,Un,Flag_Lf,P,u,Ug,xi FROM SynchronousMachine;");
        }

        #endregion
    }
}
