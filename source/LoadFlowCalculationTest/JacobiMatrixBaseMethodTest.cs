using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest
{
    [TestClass]
    public abstract class JacobiMatrixBaseMethodTest : LoadFlowCalculatorTest
    {
        [TestMethod]
        public void CalculateChangeMatrixRealPowerByAngle_OneConnection_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(1, 1);
            JacobiMatrixBasedMethod.CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0);

            Assert.AreEqual(1, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_OneConnection_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex())
            };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(1, 1);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, pqBuses);

            Assert.AreEqual(-0.99503719, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrixRealPowerByAngle_TwoConnections_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            Assert.AreEqual((-10) * v1 * v2 * Math.Cos(d2 - d1) + v1 * Math.Sin(d1), changeMatrix[0, 0], 0.001);
            Assert.AreEqual(10 * v1 * v2 * Math.Cos(d2 - d1), changeMatrix[0, 1], 0.001);
            Assert.AreEqual(10 * v1 * v2 * Math.Cos(d1 - d2), changeMatrix[1, 0], 0.001);
            Assert.AreEqual((-10) * v1 * v2 * Math.Cos(d1 - d2) + 2 * v2 * Math.Cos(d2), changeMatrix[1, 1], 0.001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_TwoConnections_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex()),
                new PQBus(1, new Complex())
            };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, pqBuses);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200) * v1 + 10 * v2 * Math.Cos(d2 - d1) - Math.Sin(d1);
            var rightUpper = 10 * v1 * Math.Cos(d2 - d1);
            var leftLower = 10 * v2 * Math.Cos(d1 - d2);
            var rightLower = 10 * v1 * Math.Cos(d1 - d2) - 400 * v2 - 2 * Math.Cos(d2);
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
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex()),
                new PQBus(1, new Complex())
            };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, pqBuses);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200) * v1 + 10 * v2 * Math.Cos(d2 - d1) - Math.Sin(d1);
            var rightUpper = 10 * v1 * Math.Cos(d2 - d1);
            var leftLower = 10 * v2 * Math.Cos(d1 - d2);
            var rightLower = 10 * v1 * Math.Cos(d1 - d2) - 400 * v2 - 2 * Math.Cos(d2);
            Assert.AreEqual(leftUpper, changeMatrix[0, 0], 0.001);
            Assert.AreEqual(rightUpper, changeMatrix[0, 1], 0.001);
            Assert.AreEqual(leftLower, changeMatrix[1, 0], 0.001);
            Assert.AreEqual(rightLower, changeMatrix[1, 1], 0.001);
        }

        [TestMethod]
        public void CalculateChangeMatrixByAngleAndAmplitude_OneConnection_CorrectResults()
        {
            var admittance = new Complex(15, 1500);
            var current = new Complex(-95.5, 1451.1);
            var voltage = new Complex(0.95, 0.15);
            var admittances = DenseMatrix.OfArray(new[,] { { admittance } });
            var constantCurrents = new DenseVector(new[] { current });
            var voltages = new DenseVector(new[] { voltage });
            var pqBuses = new List<PQBus>()
            {
                new PQBus(0, new Complex())
            };

            var changeMatrix = JacobiMatrixBasedMethod.CalculateChangeMatrixByAngleAndAmplitude(admittances, voltages, constantCurrents, pqBuses);

            var realByAngle = (-1) * voltage.Magnitude * current.Magnitude * Math.Sin(current.Phase - voltage.Phase);
            var realByAmplitude = 2 * voltage.Magnitude * admittance.Magnitude * Math.Cos(admittance.Phase) -
                                  current.Magnitude * Math.Cos(current.Phase - voltage.Phase);
            var imaginaryByAngle = (-1) * voltage.Magnitude * current.Magnitude * Math.Cos(current.Phase - voltage.Phase);
            var imaginaryByAmplitude = (-2) * voltage.Magnitude * admittance.Magnitude * Math.Sin(admittance.Phase) +
                                       current.Magnitude * Math.Sin(current.Phase - voltage.Phase);
            Assert.AreEqual(2, changeMatrix.RowCount);
            Assert.AreEqual(2, changeMatrix.ColumnCount);
            Assert.AreEqual(realByAngle, changeMatrix[0, 0], 0.0000001);
            Assert.AreEqual(realByAmplitude, changeMatrix[0, 1], 0.0000001);
            Assert.AreEqual(imaginaryByAngle, changeMatrix[1, 0], 0.0000001);
            Assert.AreEqual(imaginaryByAmplitude, changeMatrix[1, 1], 0.0000001);
        }
    }
}
