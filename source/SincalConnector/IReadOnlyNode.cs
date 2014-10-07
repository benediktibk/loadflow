using System.Numerics;

namespace SincalConnector
{
    public interface IReadOnlyNode
    {
        int Id { get; }
        string Name { get; }
        double NominalVoltage { get; }
        Complex Voltage { get; }
    }
}
