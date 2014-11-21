using System.ComponentModel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace Database
{
    public interface IReadOnlyPowerNet : INotifyPropertyChanged
    {
        int Id { get; set; }
        double Frequency { get; set; }
        string Name { get; set; }
        Selection CalculatorSelection { get; set; }
    }
}