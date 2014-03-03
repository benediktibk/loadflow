using System;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodTest : LoadFlowCalculatorSmallAccuracyTest
    {
        protected override LoadFlowCalculator CreateLoadFlowCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.01, 10000);
        }

        [TestMethod]
        public void CalculateChangeMatrixRealPowerByAngle_OneConnection_CorrectResults()
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
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_OneConnection_CorrectResults()
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
        public void CalculateChangeMatrixRealPowerByAngle_TwoConnections_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });

            var changeMatrix = FastDecoupledLoadFlowMethod.CalculateChangeMatrixRealPowerByAngle(admittances, voltages,
                constantCurrents);
            
            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            Assert.AreEqual(2, changeMatrix.RowCount);
            Assert.AreEqual(2, changeMatrix.ColumnCount);
            Assert.AreEqual((-10)*v1*v2*Math.Cos(d2 - d1) + v1*Math.Sin(d1), changeMatrix[0, 0], 0.001);
            Assert.AreEqual(10 * v1 * v2 * Math.Cos(d2 - d1), changeMatrix[0, 1], 0.001);
            Assert.AreEqual(10 * v1 * v2 * Math.Cos(d1 - d2), changeMatrix[1, 0], 0.001);
            Assert.AreEqual((-10) * v1 * v2 * Math.Cos(d1 - d2) + 2*v2*Math.Cos(d2), changeMatrix[1, 1], 0.001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_TwoConnections_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });

            var changeMatrix = FastDecoupledLoadFlowMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(admittances, voltages,
                constantCurrents);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200)*v1 + 10*v2*Math.Cos(d2 - d1) - Math.Sin(d2);
            var rightUpper = 10*v1*Math.Cos(d2 - d1);
            var leftLower = 10*v2*Math.Cos(d1 - d2);
            var rightLower = 10*v1*Math.Cos(d1 - d2) - 400*v2 - 2*Math.Cos(d2);
            Assert.AreEqual(2, changeMatrix.RowCount);
            Assert.AreEqual(2, changeMatrix.ColumnCount);
            Assert.AreEqual(leftUpper, changeMatrix[0, 0], 0.001);
            Assert.AreEqual(rightUpper, changeMatrix[0, 1], 0.001);
            Assert.AreEqual(leftLower, changeMatrix[1, 0], 0.001);
            Assert.AreEqual(rightLower, changeMatrix[1, 1], 0.001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_TwoConnectionsVersionTwo_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(0.5, 0), new Complex(0.25, 0.1) });

            var changeMatrix = FastDecoupledLoadFlowMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(admittances, voltages,
                constantCurrents);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200) * v1 + 10 * v2 * Math.Cos(d2 - d1) - Math.Sin(d2);
            var rightUpper = 10 * v1 * Math.Cos(d2 - d1);
            var leftLower = 10 * v2 * Math.Cos(d1 - d2);
            var rightLower = 10 * v1 * Math.Cos(d1 - d2) - 400 * v2 - 2 * Math.Cos(d2);
            Assert.AreEqual(2, changeMatrix.RowCount);
            Assert.AreEqual(2, changeMatrix.ColumnCount);
            Assert.AreEqual(leftUpper, changeMatrix[0, 0], 0.001);
            Assert.AreEqual(rightUpper, changeMatrix[0, 1], 0.001);
            Assert.AreEqual(leftLower, changeMatrix[1, 0], 0.001);
            Assert.AreEqual(rightLower, changeMatrix[1, 1], 0.001);
        }

        [TestMethod]
        public void CalculateVoltageChanges_OneConnection_correctResults()
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

        [TestMethod]
        public void CalculateVoltageChanges_TwoConnectionsAndCorrectVoltage_Zero()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });
            var powersRealDifference = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0, 0 });
            var powersImaginaryDifference = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 0, 0 });
            var calculator = new FastDecoupledLoadFlowMethod(0.0001, 10000);

            var voltageChange = calculator.CalculateVoltageChanges(admittances, voltages, constantCurrents,
                powersRealDifference, powersImaginaryDifference);

            Assert.AreEqual(2, voltageChange.Count);
            Assert.AreEqual(0, voltageChange[0].Magnitude, 0.0001);
            Assert.AreEqual(0, voltageChange[1].Magnitude, 0.0001);
        }
    }
}
