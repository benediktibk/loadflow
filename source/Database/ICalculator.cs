using Calculation.ThreePhase;

namespace Database
{
    public interface ICalculator
    {
        bool Calculate(SymmetricPowerNet powerNet);
    }
}
