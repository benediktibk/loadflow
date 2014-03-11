using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethodHighAccuracy : LoadFlowCalculator
    {
        private readonly double _targetPrecision;
        private readonly int _numberOfCoefficients;

        public HolomorphicEmbeddedLoadFlowMethodHighAccuracy(double targetPrecision, int numberOfCoefficients)
            : base(targetPrecision * 10000)
        {
            if (numberOfCoefficients < 1)
                throw new ArgumentOutOfRangeException("numberOfCoefficients", "must be greater or equal 1");

            if (targetPrecision < 0)
                throw new ArgumentOutOfRangeException("targetPrecision", "must be greater or equal 0");

            _numberOfCoefficients = numberOfCoefficients;
            _targetPrecision = targetPrecision;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, IList<PQBus> pqBuses,
            IList<PVBus> pvBuses)
        {
            var calculator = CreateLoadFlowCalculator(_targetPrecision, _numberOfCoefficients);
            throw new NotImplementedException();
        }

        [DllImport("C:\\Users\\benediktibk\\Entwicklung\\LoadFlow\\dev\\source\\Debug\\LoadFlowCalculationAccuracyImprovement.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int CreateLoadFlowCalculator(double targetPrecision, int numberOfCoefficients);
    }
}
