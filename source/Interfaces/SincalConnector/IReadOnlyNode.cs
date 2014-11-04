using Calculation.ThreePhase;

namespace SincalConnector
{
    public interface IReadOnlyNode
    {
        int Id { get; }
        string Name { get; }
        double NominalVoltage { get; }
        void AddTo(SymmetricPowerNet symmetricPowerNet);
    }
}
