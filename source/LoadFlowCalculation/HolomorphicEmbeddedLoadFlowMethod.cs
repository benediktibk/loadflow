using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethod :
        LoadFlowCalculator
    {
        private readonly double _coefficientTerminationCriteria;
        private readonly int _maximumNumberOfCoefficients;

        public HolomorphicEmbeddedLoadFlowMethod(double coefficientTerminationCriteria, int maximumNumberOfCoefficients)
        {
            _coefficientTerminationCriteria = coefficientTerminationCriteria;
            _maximumNumberOfCoefficients = maximumNumberOfCoefficients;
        }

        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            throw new NotImplementedException();
        }
    }
}
