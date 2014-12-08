using Calculation.ThreePhase;

namespace SincalConnector
{
    public interface INetElement
    {
        int Id { get; }
        void AddTo(SymmetricPowerNet powerNet, double powerFactor);
    }
}
