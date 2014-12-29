using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;

namespace SincalConnector
{
    public interface IReadOnlyConnectorData
    {
        int SizeOfDataType { get; }
        int CountOfCoefficients { get; }
        double TargetPrecision { get; }
        int MaximumIterations { get; }
        Selection CalculatorSelection { get; }
        double Progress { get; }
        double RelativePowerError { get; }
        string InputFile { get; }
        INodeVoltageCalculator CreateCalculator();
    }
}