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
            var modelType = record.Parse<int>("Flag_Lf");

            if (modelType != 1 && modelType != 2)
                throw new NotSupportedException("not supported load model");

            NodeId = nodeIdsByElementIds.GetOnly(Id);
            var p = record.Parse<double>("P") * 1e6;
            var q = record.Parse<double>("Q") * 1e6;
            LoadValue = new Complex(p, q)*(-1);
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
