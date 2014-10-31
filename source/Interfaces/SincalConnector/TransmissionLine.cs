using System;
using System.Data.OleDb;
using System.IO;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class TransmissionLine : INetElement
    {
        #region constructor

        public TransmissionLine(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds, double frequency)
        {
            Id = record.Parse<int>("Element_ID");
            var lineType = record.Parse<int>("Flag_LineTyp");

            if (lineType != 1 && lineType != 2)
                throw new NotSupportedException("the selected transmission line type is not supported");

            Length = record.Parse<double>("l")*1000;
            SeriesResistancePerUnitLength = record.Parse<double>("r") / 1000;
            SeriesInductancePerUnitLength = record.Parse<double>("x") / (2 * Math.PI * frequency * 1000);
            var shuntLosses = record.Parse<double>("va");
            var nominalVoltage = record.Parse<double>("Un")*1000;
            ShuntConductancePerUnitLength = shuntLosses / (nominalVoltage * nominalVoltage);
            ShuntCapacityPerUnitLength = record.Parse<double>("c") / 1e12;
            TransmissionEquationModel = record.Parse<int>("Flag_Ll") == 1;
            var nodes = nodeIdsByElementIds.Get(Id);

            if (nodes.Count != 2)
                throw new InvalidDataException("a transmission line must have exactly two connected nodes");

            NodeOneId = nodes[0];
            NodeTwoId = nodes[1];

            if (Math.Abs(frequency - record.Parse<double>("fn")) > 0.00001)
                throw new NotSupportedException("the frequency of a transmission line does not match the net frequency");

            var parallelSystems = record.Parse<double>("ParSys");

            if (parallelSystems != 1)
                throw new NotSupportedException("parallel transmission lines are not supported");
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeOneId { get; private set; }
        public int NodeTwoId { get; private set; }
        public double SeriesResistancePerUnitLength { get; private set; }
        public double SeriesInductancePerUnitLength { get; private set; }
        public double ShuntConductancePerUnitLength { get; private set; }
        public double ShuntCapacityPerUnitLength { get; private set; }
        public double Length { get; private set; }
        public bool TransmissionEquationModel { get; private set; }

        #endregion

        #region public functions

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddTransmissionLine(NodeOneId, NodeTwoId, SeriesResistancePerUnitLength,
                SeriesInductancePerUnitLength, ShuntConductancePerUnitLength, ShuntCapacityPerUnitLength, Length,
                TransmissionEquationModel);
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_LineTyp,Flag_Ll,l,ParSys,fr,r,x,c,va,fn,Un FROM Line;");
        }

        #endregion
    }
}
