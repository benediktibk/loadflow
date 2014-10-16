using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Numerics;
using Calculation.ThreePhase;
using DatabaseHelper;

namespace SincalConnector
{
    public class SlackGenerator : INetElement
    {
        #region constructor

        public SlackGenerator(ISafeDatabaseRecord record, IReadOnlyDictionary<int, IReadOnlyNode> nodes, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var machineType = record.Parse<int>("Flag_Machine");
            var nominalVoltage = record.Parse<double>("Un") * 1000;
            var loadFlowType = record.Parse<int>("Flag_Lf");
            var relativeSynchronousReactance = record.Parse<double>("xi") / 100;
            var nominalPower = record.Parse<double>("Sn") * 1e6;
            double voltageMagnitude;
            SynchronousReactance = relativeSynchronousReactance * nominalVoltage * nominalVoltage / nominalPower;

            if (machineType != 1)
                throw new NotSupportedException("the selected machine type for a generator is not supported");

            if (Math.Abs(nominalVoltage - nodes[NodeId].NominalVoltage) > 0.000001)
                throw new InvalidDataException(
                    "the nominal voltage of a generator does not match the nominal voltage of the connected node");

            switch (loadFlowType)
            {
                case 3:
                    voltageMagnitude = nominalVoltage * record.Parse<double>("u") / 100;
                    break;
                case 5:
                    voltageMagnitude = record.Parse<double>("Ug") * 1000;
                    break;
                default:
                    throw new NotSupportedException("the selected load flow type of a generator is not supported");
            }

            var delta = record.Parse<double>("delta")*Math.PI/180;
            Voltage = Complex.FromPolarCoordinates(voltageMagnitude, delta);
        }

        #endregion

        #region properties

        public int Id { get; private set; }
        public int NodeId { get; private set; }
        public double SynchronousReactance { get; private set; }
        public Complex Voltage { get; private set; }

        #endregion

        #region public functions

        public void AddTo(SymmetricPowerNet powerNet)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region static functions

        public static OleDbCommand CreateCommandToFetchAll()
        {
            return new OleDbCommand("SELECT Element_ID,Flag_Machine,Un,Flag_Lf,u,Ug,xi,Sn,delta FROM SynchronousMachine WHERE Flag_Lf = 3 OR Flag_Lf = 5;");
        }

        #endregion
    }
}
