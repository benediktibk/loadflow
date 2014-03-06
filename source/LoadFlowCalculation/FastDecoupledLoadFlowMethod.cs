using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, 0.1, targetPrecision*1E6)
        { }

        public override Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, Vector<double> powersRealError,
            Vector<double> powersImaginaryError)
        {
            var nodeCount = admittances.RowCount;
            var changeMatrixRealPower = new DenseMatrix(nodeCount, nodeCount);
            CalculateChangeMatrixRealPowerByAngle(changeMatrixRealPower, admittances, voltages, constantCurrents, 0, 0);
            var changeMatrixImaginaryPower = new DenseMatrix(nodeCount, nodeCount);
            CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrixImaginaryPower, admittances, voltages, constantCurrents, 0, 0);
            var factorizationRealPower = changeMatrixRealPower.QR();
            var factorizationImaginaryPower = changeMatrixImaginaryPower.QR();
            var angleChange = factorizationRealPower.Solve(powersRealError);
            var amplitudeChange = factorizationImaginaryPower.Solve(powersImaginaryError);
            var voltageChanges = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                voltageChanges[i] = Complex.FromPolarCoordinates(amplitudeChange[i], angleChange[i]);

            return voltageChanges;
        }
    }
}
