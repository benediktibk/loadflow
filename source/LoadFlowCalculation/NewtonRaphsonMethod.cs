using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NewtonRaphsonMethod : LoadFlowCalculator
    {
        public override Vector<Complex> CalculateUnknownVoltages(Matrix<Complex> admittances, double nominalVoltage, Vector<Complex> constantCurrents, Vector<Complex> knownPowers,
            out bool voltageCollapse)
        {
            throw new NotImplementedException();
        }
    }
}
