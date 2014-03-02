using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace LoadFlowCalculation
{
    public class FastDecoupledLoadFlowMethod : JacobiMatrixBasedMethod
    {
        public FastDecoupledLoadFlowMethod(double targetPrecision, int maximumIterations) : base(targetPrecision, maximumIterations, 1, -0.1)
        { }

        protected override Vector<Complex> CalculateVoltageChanges(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents, Vector<double> powersRealError,
            Vector<double> powersImaginaryError)
        {
            var changeMatrixRealPower = CalculateChangeMatrixRealPowerByAngle(admittances, voltages, constantCurrents);
            var changeMatrixImaginaryPower = CalculateChangeMatrixImaginaryPowerByAmplitude(admittances, voltages, constantCurrents);
            var factorizationRealPower = changeMatrixRealPower.QR();
            var factorizationImaginaryPower = changeMatrixImaginaryPower.QR();
            var angleChange = factorizationRealPower.Solve(powersRealError);
            var amplitudeChange = factorizationImaginaryPower.Solve(powersImaginaryError);
            var nodeCount = admittances.RowCount;
            var voltageChanges = new MathNet.Numerics.LinearAlgebra.Complex.DenseVector(nodeCount);

            for (var i = 0; i < nodeCount; ++i)
                voltageChanges[i] = Complex.FromPolarCoordinates(amplitudeChange[i], angleChange[i]);

            return voltageChanges;
        }

        public static Matrix<double> CalculateChangeMatrixRealPowerByAngle(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents)
        {
            var nodeCount = admittances.RowCount;
            var changeMatrix = new DenseMatrix(nodeCount, nodeCount);

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
                        var voltageTwoAmplitude = voltageTwo.Magnitude;
                        var voltageTwoAngle = voltageTwo.Phase;

                        changeMatrix[i, j] = (1)*admittanceAmplitude*voltageOneAmplitude*voltageTwoAmplitude*
                                             Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;
                        var constantCurrentPart = voltageOneAmplitude*currentMagnitude*
                                                  Math.Sin(currentAngle - voltageOneAngle);

                        changeMatrix[i, j] = (-1)*constantCurrentPart;
                    }
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += changeMatrix[i, j];

                changeMatrix[i, i] += sum;
            }

            return changeMatrix;
        }

        public static Matrix<double> CalculateChangeMatrixImaginaryPowerByAmplitude(Matrix<Complex> admittances, Vector<Complex> voltages, Vector<Complex> constantCurrents)
        {
            var nodeCount = admittances.RowCount;
            var changeMatrix = new DenseMatrix(nodeCount, nodeCount);

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

                        changeMatrix[i, j] = (1) * admittanceAmplitude * voltageOneAmplitude *
                                             Math.Sin(admittanceAngle + voltageTwoAngle - voltageOneAngle);
                    }
                    else
                    {
                        var current = constantCurrents[i];
                        var currentMagnitude = current.Magnitude;
                        var currentAngle = current.Phase;

                        changeMatrix[i, i] = currentMagnitude*Math.Sin(currentAngle - voltageOneAngle) -
                                             2*admittanceAmplitude*voltageOneAmplitude*Math.Sin(admittanceAngle);
                    }
                }
            }

            for (var i = 0; i < nodeCount; ++i)
            {
                double sum = 0;

                for (var j = 0; j < nodeCount; ++j)
                    if (i != j)
                        sum += changeMatrix[i, j];

                changeMatrix[i, i] += sum;
            }

            return changeMatrix;
        }
    }
}
