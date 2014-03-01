
using System;
using System.Collections.Generic;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations)
        { }

        protected override Vector<double> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<double> voltagesReal, Vector<double> voltagesImaginary,
            Vector<double> constantCurrentsReal, Vector<double> constantCurrentsImaginary, Vector<double> powersReal, Vector<double> lastPowersReal,
            Vector<double> powersImaginary, Vector<double> lastPowersImaginary)
        {
            throw new NotImplementedException();
        }
    }
}
