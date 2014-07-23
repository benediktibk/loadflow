using System;
using System.Numerics;

namespace Calculation.SinglePhase.MultipleVoltageLevels
{
    public class DimensionScaler
    {
        #region member variables

        private readonly double _voltageBase;
        private readonly double _powerBase;
        private readonly double _currentBase;
        private readonly double _impedanceBase;

        #endregion

        #region public functions

        public DimensionScaler(double voltageBase, double powerBase)
        {
            if (voltageBase < 0.001)
                throw new ArgumentOutOfRangeException("voltageBase", "is too small");

            if (powerBase < 0.001)
                throw new ArgumentOutOfRangeException("powerBase", "is too small");

            _voltageBase = voltageBase;
            _powerBase = powerBase;
            _currentBase = powerBase/voltageBase;
            _impedanceBase = voltageBase/_currentBase;
        }

        public double ScaleVoltage(double voltage)
        {
            return voltage/VoltageBase;
        }

        public Complex ScaleVoltage(Complex voltage)
        {
            return voltage/VoltageBase;
        }

        public Complex UnscaleVoltage(Complex voltage)
        {
            return voltage*VoltageBase;
        }

        public double ScalePower(double power)
        {
            return power / PowerBase;
        }

        public Complex ScalePower(Complex power)
        {
            return power/PowerBase;
        }

        public double ScaleCurrent(double current)
        {
            return current/CurrentBase;
        }

        public Complex ScaleCurrent(Complex current)
        {
            return current / CurrentBase;
        }

        public double ScaleImpedance(double impedance)
        {
            return impedance / ImpedanceBase;
        }

        public Complex ScaleImpedance(Complex impedance)
        {
            return impedance / ImpedanceBase;
        }

        public double ScaleAdmittance(double admittance)
        {
            return admittance * ImpedanceBase;
        }

        public Complex ScaleAdmittance(Complex admittance)
        {
            return admittance * ImpedanceBase;
        }

        #endregion

        #region public properties

        public double VoltageBase
        {
            get { return _voltageBase; }
        }

        public double PowerBase
        {
            get { return _powerBase; }
        }

        public double CurrentBase
        {
            get { return _currentBase; }
        }

        public double ImpedanceBase
        {
            get { return _impedanceBase; }
        }

        #endregion
    }
}
