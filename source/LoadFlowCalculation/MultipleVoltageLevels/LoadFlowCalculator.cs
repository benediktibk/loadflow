using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace LoadFlowCalculation.MultipleVoltageLevels
{
    public class LoadFlowCalculator
    {
        #region variables

        private readonly double _scaleBasisVoltage;
        private readonly double _scaleBasisPower;
        private readonly double _scaleBasisCurrent = 0;
        private readonly double _scaleBasisImpedance = 0;

        #endregion

        #region public functions

        public LoadFlowCalculator(double scaleBasisVoltage, double scaleBasisPower)
        {
            _scaleBasisVoltage = scaleBasisVoltage;
            _scaleBasisPower = scaleBasisPower;
            _scaleBasisCurrent = scaleBasisPower/scaleBasisVoltage;
            _scaleBasisImpedance = _scaleBasisVoltage/_scaleBasisCurrent;
        }

        #endregion

        #region public properties

        public double ScaleBasisVoltage
        {
            get { return _scaleBasisVoltage; }
        }

        public double ScaleBasisPower
        {
            get { return _scaleBasisPower; }
        }

        public double ScaleBasisCurrent
        {
            get { return _scaleBasisCurrent; }
        }

        public double ScaleBasisImpedance
        {
            get { return _scaleBasisImpedance; }
        }

        #endregion

        #region private functions
        #endregion
    }
}
