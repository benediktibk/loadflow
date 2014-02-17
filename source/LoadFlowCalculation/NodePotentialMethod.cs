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
        public Vector calculateNodeVoltages(Matrix admittances, Vector powers, Complex nominalVoltage, List<Tuple<uint, Complex>> knownVoltages)
        {
            LU factorization = admittances.LU();
            Vector currents = new DenseVector(powers / nominalVoltage);
            //return factorization.Solve(currents);
            return currents;
        }
    }
}
