
namespace CalculationComparison
{
    public class CombinedCalculationResult
    {
        public string Algorithm { get; set; }
        public bool VoltageCollapse { get; set; }
        public bool VoltageCollapseDetected { get; set; }
        public double RelativePowerError { get; set; }
        public double MaximumRelativeVoltageError { get; set; }
        public double AverageExecutionTime { get; set; }
        public double StandardDeviationExecutionTime { get; set; }
    }
}
