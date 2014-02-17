using System.Collections.Generic;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public interface ILoadFlowCalculator
    {
        List<Node> calculateNodeVoltages(Matrix admittances, double nominalVoltage, List<Node> nodes);
    }
}
