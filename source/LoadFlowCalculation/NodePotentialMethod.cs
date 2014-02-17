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
        public List<Node> calculateNodeVoltages(Matrix admittances, double nominalVoltage, List<Node> nodes)
        {
            throw new NotImplementedException();
        }
    }
}
