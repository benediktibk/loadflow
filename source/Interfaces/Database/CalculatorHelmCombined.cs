using System;
using System.Collections.Generic;
using Calculation;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

namespace Database
{
    public class CalculatorHelmCombined : ICalculator
    {
        private readonly INodeVoltageCalculator _iterativeNodeVoltageCalculator;
        private readonly INodeVoltageCalculator _helmCombined;
        private readonly INodeVoltageCalculator _helmMulti;
        private readonly LogFunction _log;

        public CalculatorHelmCombined(INodeVoltageCalculator iterativeNodeVoltageCalculator,
            int coefficientCountHelmSecondStep, int bitPrecisionSecondStep,
            double targetPrecision, LogFunction log)
        {
            if (iterativeNodeVoltageCalculator == null)
                throw new ArgumentNullException("iterativeNodeVoltageCalculator");

            if (log == null)
                throw new ArgumentNullException("log");

            _iterativeNodeVoltageCalculator = iterativeNodeVoltageCalculator;
            _helmCombined = new HolomorphicEmbeddedLoadFlowMethodWithIterativeMethod(targetPrecision,
                iterativeNodeVoltageCalculator);
            _helmMulti = new HolomorphicEmbeddedLoadFlowMethod(targetPrecision, coefficientCountHelmSecondStep, new PrecisionMulti(bitPrecisionSecondStep));
            _log = log;
        }

        public delegate void LogFunction(string message);

        public IReadOnlyDictionary<long, NodeResult> Calculate(SymmetricPowerNet powerNet)
        {
            _log("trying to calculate the node voltages with the iterative method");
            var nodeResults = powerNet.CalculateNodeVoltages(_iterativeNodeVoltageCalculator);
            if (nodeResults != null)
                return nodeResults;

            _log("iterative method reported voltage collapse, trying HELM combined with the iterative method");
            nodeResults = powerNet.CalculateNodeVoltages(_helmCombined);
            if (nodeResults != null)
                return nodeResults;

            _log("HELM combined with the iterative method reported voltage collapse, trying HELM with multi precision");
            return powerNet.CalculateNodeVoltages(_helmMulti);
        }
    }
}
