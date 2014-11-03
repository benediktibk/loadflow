using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;

namespace Calculation
{
    public class NodeResult
    {
        public NodeResult(Complex voltage, Complex power)
        {
            Voltage = voltage;
            Power = power;
        }

        public Complex Voltage { get; private set; }
        public Complex Power { get; private set; }

        public NodeResult Unscale(double voltageBase, double powerBase)
        {
            var scaler = new DimensionScaler(voltageBase, powerBase);
            return new NodeResult(scaler.UnscaleVoltage(Voltage), scaler.UnscalePower(Power));
        }
    }
}
