﻿
namespace CalculationComparison
{
    public class CalculationResult
    {
        public string Algorithm { get; set; }
        public bool VoltageCollapse { get; set; }
        public bool VoltageCollapseDetected { get; set; }
        public double RelativePowerError { get; set; }
        public double MaximumRelativeVoltageError { get; set; }
        public double ExecutionTime { get; set; }
    }
}