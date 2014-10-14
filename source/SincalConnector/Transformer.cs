using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using DatabaseHelper;

namespace SincalConnector
{
    public class Transformer
    {
        #region constructor

        public Transformer(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NominalPower = record.Parse<double>("Sn")*1e6;
            RelativeShortCircuitVoltage = record.Parse<double>("uk")/100;
            IronLosses = record.Parse<double>("Vfe")*1000;
            RelativeNoLoadCurrent = record.Parse<double>("i0")/100;
            var realRelativeShortCircuitVoltge = record.Parse<double>("ur")/100;
            CopperLosses = NominalPower*realRelativeShortCircuitVoltge;
            Ratio = 1;
            var controlStage = record.Parse<double>("roh");

            if (controlStage != 0)
                throw new NotSupportedException("control stages are not supported");

            var connectedNodes = nodeIdsByElementIds.Get(Id);

            if (connectedNodes.Count != 2)
                throw new InvalidDataException("a transformer must be connected to two nodes");

            var firstNodeVoltage = nodes[connectedNodes[0]].NominalVoltage;
            var secondNodeVoltage = nodes[connectedNodes[1]].NominalVoltage;

            if (firstNodeVoltage > secondNodeVoltage)
            {
                UpperSideNodeId = connectedNodes[0];
                LowerSideNodeId = connectedNodes[1];
            }
            else
            {
                UpperSideNodeId = connectedNodes[1];
                LowerSideNodeId = connectedNodes[0];
            }

            var connectionSymbol = record.Parse<int>("VecGrp");
            var phaseShiftFactor = MapConnectionSymbolToPhaseShiftFactor(connectionSymbol);
            PhaseShift = phaseShiftFactor*30*Math.PI/180;
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int UpperSideNodeId { get; private set; }
        public int LowerSideNodeId { get; private set; }
        public double NominalPower { get; private set; }
        public double RelativeShortCircuitVoltage { get; private set; }
        public double CopperLosses { get; private set; }
        public double IronLosses { get; private set; }
        public double RelativeNoLoadCurrent { get; private set; }
        public double Ratio { get; private set; }
        public double PhaseShift { get; private set; }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Sn,uk,ur,Vfe,i0,VecGrp,roh FROM TwoWindingTransformer;");
        }

        public static int MapConnectionSymbolToPhaseShiftFactor(int connectionSymbol)
        {
            if ((connectionSymbol >= 1 && connectionSymbol <= 9) ||
                connectionSymbol == 71 || connectionSymbol == 72 ||
                connectionSymbol == 73 || connectionSymbol == 76 ||
                connectionSymbol == 77)
                return 0;
            if ((connectionSymbol >= 10 && connectionSymbol <= 22) ||
                connectionSymbol == 70 || connectionSymbol == 74 ||
                connectionSymbol == 78 || connectionSymbol == 80)
                return 1;
            if (connectionSymbol >= 23 && connectionSymbol <= 34)
                return 5;
            if (connectionSymbol >= 35 && connectionSymbol <= 43)
                return 6;
            if ((connectionSymbol >= 44 && connectionSymbol <= 57) ||
                connectionSymbol == 75)
                return 7;
            if ((connectionSymbol >= 58 && connectionSymbol <= 69) ||
                connectionSymbol == 79 || connectionSymbol == 81)
                return 11;

            throw new ArgumentOutOfRangeException("connectionSymbol");
        }

        #endregion
    }
}
