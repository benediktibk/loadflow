using System.Collections.Generic;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public interface ILoadFlowCalculator
    {
        Vector calculateNodeVoltages(Matrix admittances, Vector powers, Complex nominalVoltage, List<Tuple<uint, Complex>> knownVoltages);
    }
}
