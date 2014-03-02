
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

        protected override Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, Vector<double> powersRealError,
            Vector<double> powersImaginaryError)
        {
            throw new NotImplementedException();
        }
    }
}
