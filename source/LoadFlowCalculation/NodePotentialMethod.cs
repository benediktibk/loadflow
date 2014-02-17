using System.Numerics;
using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Factorization;

namespace LoadFlowCalculation
{
    public class NodePotentialMethod :
        ILoadFlowCalculator
    {
        public Dictionary<uint, Node> CalculateNodeVoltages(Matrix admittances, double nominalVoltage, Dictionary<uint, Node> nodes)
        {
            throw new NotImplementedException();
        }
    }
}
