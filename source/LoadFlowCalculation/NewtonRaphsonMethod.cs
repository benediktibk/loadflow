using System.Collections.Generic;
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
            var voltagesReal = ExtractRealParts(voltages);
            var voltagesImaginary = ExtractImaginaryParts(voltages);
            var constantCurrentsReal = ExtractRealParts(constantCurrents);
            var constantCurrentsImaginary = ExtractImaginaryParts(constantCurrents);
            var changeMatrix = CalculateChangeMatrixByRealAndImaginaryPart(admittances, voltagesReal, voltagesImaginary,
                constantCurrentsReal, constantCurrentsImaginary);
            var rightSide = CombineParts(powersRealError, powersImaginaryError);
            var factorization = changeMatrix.QR();
            var voltageChanges = factorization.Solve(rightSide);
            Vector<double> voltageChangesReal;
            Vector<double> voltageChangesImaginary;
            DivideParts(voltageChanges, out voltageChangesReal, out voltageChangesImaginary);
            return CombineRealAndImaginaryParts(voltageChangesReal, voltageChangesImaginary);
        }
    }
}
