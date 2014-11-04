using System.Numerics;
using Calculation.ThreePhase;

namespace SincalConnector
{
    public interface IReadOnlyNode
    {
        int Id { get; }
        string Name { get; }
        double NominalVoltage { get; }
        Complex Voltage { get; }
        Complex Load { get; }
        void AddTo(SymmetricPowerNet symmetricPowerNet);
    }
}
