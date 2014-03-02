using System;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodTest : LoadFlowCalculatorHighAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.001, 10000);
        }

        [TestMethod]
        public void CalculateChangeMatrixRealPowerByAngle_oneConnection_correctResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] {{new Complex(10, 0)}});
            var constantCurrents = new DenseVector(new[] {new Complex(10, 0)});
            var voltages = new DenseVector(new[] {new Complex(1, 0.1)});

            var changeMatrix = FastDecoupledLoadFlowMethod.CalculateChangeMatrixRealPowerByAngle(admittances, voltages,
                constantCurrents);

            Assert.AreEqual(1, changeMatrix.RowCount);
            Assert.AreEqual(1, changeMatrix.ColumnCount);
            Assert.AreEqual(1, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_oneConnection_correctResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });

            var changeMatrix = FastDecoupledLoadFlowMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(admittances, voltages,
                constantCurrents);

            Assert.AreEqual(1, changeMatrix.RowCount);
            Assert.AreEqual(1, changeMatrix.ColumnCount);
            Assert.AreEqual(-0.99503719, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateVoltageChanges_oneConnection_correctResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });
            var powersReal = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -1 });
            var powersImaginary = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0 });
            var voltageAmplitude = voltages[0].Magnitude;
            var voltageAngle = voltages[0].Phase;
            var currentPowerReal = 10*voltageAmplitude*voltageAmplitude - 10*voltageAmplitude*Math.Cos(voltageAngle);
            var currentPowerImaginary = (-10)*voltageAmplitude*Math.Sin(voltageAngle);
            var powerRealDifference = powersReal -
                                      new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new[] { currentPowerReal });
            var powerImaginaryDifference = powersImaginary -
                                      new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new[] { currentPowerImaginary });
            var calculator = new FastDecoupledLoadFlowMethod(0.0001, 10000);

            var voltageChange = calculator.CalculateVoltageChanges(admittances, voltages, constantCurrents,
                powerRealDifference, powerImaginaryDifference);

            var derivationRealPowerByAngle = 10*voltageAmplitude*Math.Sin(voltageAngle);
            var derivationImaginaryPowerByAmplitude = (-10)*Math.Sin(voltageAngle);
            var voltageChangeShouldBe =
                Complex.FromPolarCoordinates(powerImaginaryDifference[0]/derivationImaginaryPowerByAmplitude,
                    powerRealDifference[0]/derivationRealPowerByAngle);
            Assert.AreEqual(1, voltageChange.Count);
            ComplexAssert.AreEqual(voltageChangeShouldBe, voltageChange[0], 0.00001);
        }
    }
}
