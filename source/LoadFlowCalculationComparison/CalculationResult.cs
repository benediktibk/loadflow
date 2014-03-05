
namespace LoadFlowCalculationComparison
{
    class CalculationResult
    {
        public string Algorithm { get; set; }
        public bool VoltageCollapse { get; set; }
        public bool VoltageCollapseDetected { get; set; }
        public double RelativePowerError { get; set; }
        public double MaximumVoltageError { get; set; }
        public double AverageExecutionTime { get; set; }
        public double VarianceExecutionTime { get; set; }
    }
}
