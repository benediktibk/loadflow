using Calculation.ThreePhase;
using Misc;

namespace SincalConnector
{
    public class Node : IReadOnlyNode
    {
        public Node(ISafeDatabaseRecord record)
        {
            var nameFull = record.Parse<string>("Name");
            var nameLength = nameFull.Length;

            while (nameLength > 0 && nameFull[nameLength - 1] == ' ')
                --nameLength;

            Id = record.Parse<int>("Id");
            Name = nameFull.Substring(0, nameLength);
            NominalVoltage = record.Parse<double>("Un")*1000;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public double NominalVoltage { get; private set; }

        public void AddTo(SymmetricPowerNet powerNet)
        {
            powerNet.AddNode(Id, NominalVoltage, Name);
        }
    }
}
