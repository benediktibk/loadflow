using System.Collections.Generic;
using System.Numerics;
using Calculation;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    public class PowerNetTestCase
    {
        private readonly PowerNetComputable _powerNet;
        private readonly Vector<Complex> _correctVoltages;
        private readonly Vector<Complex> _correctPowers;

        public PowerNetTestCase(PowerNetComputable powerNet, Vector<Complex> correctVoltages,
            Vector<Complex> correctPowers)
        {
            _powerNet = powerNet;
            _correctVoltages = correctVoltages;
            _correctPowers = correctPowers;
        }

        public Vector<Complex> CorrectVoltages
        {
            get { return _correctVoltages; }
        }

        public Vector<Complex> CorrectPowers
        {
            get { return _correctPowers; }
        }

        public IList<NodeResult> CalculateNodeResults()
        {
            return _powerNet.CalculateNodeResults();
        }
    }
}
