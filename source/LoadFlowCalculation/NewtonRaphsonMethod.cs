using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class NewtonRaphsonMethod : JacobiMatrixBasedMethod
    {
        public NewtonRaphsonMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, 0, targetPrecision*100)
        { }

        public override Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages,
            Vector<Complex> constantCurrents, Vector<double> powersRealError, Vector<double> powersImaginaryError)
        {
            var changeMatrix = CalculateChangeMatrix(admittances, voltages, constantCurrents);
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var factorization = changeMatrix.QR();
            var voltageChanges = factorization.Solve(rightSide);
            Vector<double> voltageChangesAngle;
            Vector<double> voltageChangesAmplitude;
            DivideParts(voltageChanges, out voltageChangesAngle, out voltageChangesAmplitude);
            return CombineAmplitudesAndAngles(voltageChangesAmplitude, voltageChangesAngle);
        }

        public static Matrix<double> CalculateChangeMatrix(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents)
        {
            var nodeCount = admittances.RowCount;
            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(nodeCount * 2, nodeCount * 2);

            CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0);
            CalculateChangeMatrixRealPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, nodeCount);
            CalculateChangeMatrixImaginaryPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, nodeCount, 0);
            CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, nodeCount, nodeCount);

            return changeMatrix;
        }
    }
}
