using System;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using Calculation.ThreePhase;

namespace Database
{
    public class CalculatorHelmCombined : ICalculator
    {
        #region variables

        private readonly INodeVoltageCalculator _iterativeNodeVoltageCalculator;
        private readonly INodeVoltageCalculator _helmCombined;
        private readonly INodeVoltageCalculator _helmMulti;
        private readonly LogFunction _log;

        #endregion

        #region definitions

        public delegate void LogFunction(string message);

        #endregion

        #region constructor

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

        #endregion

        #region ICalculator

        public bool Calculate(SymmetricPowerNet powerNet)
        {
            _log("trying to calculate the node voltages with the iterative method");
            if (powerNet.CalculateNodeVoltages(_iterativeNodeVoltageCalculator))
                return true;

            _log("iterative method reported voltage collapse, trying HELM combined with the iterative method");
            if (powerNet.CalculateNodeVoltages(_helmCombined))
                return true;

            _log("HELM combined with the iterative method reported voltage collapse, trying HELM with multi precision");
            return powerNet.CalculateNodeVoltages(_helmMulti);
        }

        #endregion
    }
}
