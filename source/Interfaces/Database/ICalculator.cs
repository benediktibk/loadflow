using System.Collections.Generic;
using Calculation;
using Calculation.ThreePhase;

namespace Database
{
    public interface ICalculator
    {
        IReadOnlyDictionary<long, NodeResult> Calculate(SymmetricPowerNet powerNet);
    }
}
