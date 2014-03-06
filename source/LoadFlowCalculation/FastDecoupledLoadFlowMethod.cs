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

        public static void CalculateChangeMatrixRealPowerByAngle(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int row, int column)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageOne = voltages[i];
                var voltageOneAmplitude = voltageOne.Magnitude;
                var voltageOneAngle = voltageOne.Phase;

                for (var j = 0; j < nodeCount; ++j)
                {
                    if (i != j)
                    {
                        var voltageTwo = voltages[j];
                        var voltageTwoAmplitude = voltageTwo.Magnitude;
                        var voltageTwoAngle = voltageTwo.Phase;
                        var admittance = admittances[i, j];
                        var admittanceAmplitude = admittance.Magnitude;
                        var admittanceAngle = admittance.Phase;
                        var sine = Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);

                        result[row + i, column + j] = (1) * admittanceAmplitude * voltageOneAmplitude * voltageTwoAmplitude * sine;
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;
                        var constantCurrentPart = voltageOneAmplitude*currentMagnitude*
                                                  Math.Sin(currentAngle - voltageOneAngle);

                        result[row + i, column + j] = (-1) * constantCurrentPart;
                    }
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += result[row + i, column + j];

                result[row + i, column + i] = result[row + i, column + i] + sum;
            }
        }

        public static void CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<double> result, Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, int row, int column)
        {
            var nodeCount = admittances.RowCount;

            for (var i = 0; i < nodeCount; ++i)
            {
                var voltageOne = voltages[i];
                var voltageOneAmplitude = voltageOne.Magnitude;
                var voltageOneAngle = voltageOne.Phase;

                for (var j = 0; j < nodeCount; ++j)
                {
                    var admittance = admittances[i, j];
                    var admittanceAmplitude = admittance.Magnitude;
                    var admittanceAngle = admittance.Phase;

                    if (i != j)
                    {
                        var voltageTwo = voltages[j];
                        var voltageTwoAngle = voltageTwo.Phase;

                        result[row + i, column + j] = (-1) * admittanceAmplitude * voltageOneAmplitude *
                                             Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;

                        result[row + i, column + j] = currentMagnitude * Math.Sin(currentAngle - voltageOneAngle) -
                                             2*admittanceAmplitude*voltageOneAmplitude*Math.Sin(admittanceAngle);
                    }
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += result[row + j, column + i];

                result[row + i, column + i] = result[row + i, column + i] + sum;
            }
        }
    }
}
