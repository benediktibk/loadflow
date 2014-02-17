using System.Collections.Generic;
using System.Numerics;
using System;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace LoadFlowCalculation
{
    public interface ILoadFlowCalculator
    {
        Dictionary<uint, Node> CalculateNodeVoltages(Matrix admittances, double nominalVoltage, Dictionary<uint, Node> nodes);
    }
}
