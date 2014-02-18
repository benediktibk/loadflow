using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class HolomorphicEmbeddedLoadFlowMethod :
        LoadFlowCalculator
    {
        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances,
            double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers)
        {
            throw new NotImplementedException();
        }
    }
}
