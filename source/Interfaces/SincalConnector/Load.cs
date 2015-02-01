using System;
using System.Numerics;
using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class Load : INetElement
    {
        public Load(ISafeDatabaseRecord record, IReadOnlyMultiDictionary<int, int> nodeIdsByElementIds)
        {
            Id = record.Parse<int>("Element_ID");
            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var modelType = record.Parse<int>("Flag_Lf");

            switch (modelType)
            {
                case 1:
                case 2:
                    var p = record.Parse<double>("P") * 1e6;
                    var q = record.Parse<double>("Q") * 1e6;
                    LoadValue = new Complex(p, q) * (-1);
                    break;
                case 9:
                    var wp = record.Parse<double>("Eap") * 1e3;
                    var wq = record.Parse<double>("Erp") * 1e3;
                    const int hoursPerYear = 365*24;
                    LoadValue = new Complex(wp, wq)/hoursPerYear*(-1);
                    break;
                default:
                    throw new NotSupportedException("not supported load model");
            }
        }

        public Load(int nodeId, Complex loadValue)
        {
            Id = -1;
            NodeId = nodeId;
            LoadValue = loadValue;
        }

        public int Id { get; private set; }

        public int NodeId { get; private set; }

        public Complex LoadValue { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet, double powerFactor)
        {
            powerNet.AddLoad(NodeId, LoadValue*powerFactor);
        }
    }
}
