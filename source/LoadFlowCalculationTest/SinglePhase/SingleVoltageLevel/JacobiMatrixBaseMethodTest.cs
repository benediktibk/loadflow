using System;
using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoadFlowCalculationTest.SinglePhase.SingleVoltageLevel
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
            var rows = new List<int>() { 0 };
            var columns = new List<int>() { 0 };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(1, 1);
            JacobiMatrixBasedMethod.CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0, rows, columns);

            Assert.AreEqual(1, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_OneConnection_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(10, 0) } });
            var constantCurrents = new DenseVector(new[] { new Complex(10, 0) });
            var voltages = new DenseVector(new[] { new Complex(1, 0.1) });
            var rows = new List<int>() { 0 };
            var columns = new List<int>() { 0 };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(1, 1);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, rows, columns);

            Assert.AreEqual(-0.99503719, changeMatrix[0, 0], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrixRealPowerByAngle_TwoConnections_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -1) });
            var rows = new List<int>() { 0, 1 };
            var columns = new List<int>() { 0, 1 };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixRealPowerByAngle(changeMatrix, admittances, voltages, constantCurrents, 0, 0, rows, columns);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            Assert.AreEqual(2, changeMatrix.RowCount);
            Assert.AreEqual(2, changeMatrix.ColumnCount);
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
            var rows = new List<int>() { 0, 1 };
            var columns = new List<int>() { 0, 1 };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, rows, columns);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200) * v1 + 10 * v2 * Math.Cos(d2 - d1) - Math.Sin(d1);
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
        public void CalculateChangeMatrixImaginaryPowerByAmplitude_TwoConnectionsVersionTwo_CorrectResults()
        {
            var admittances = DenseMatrix.OfArray(new[,] { { new Complex(0, 100), new Complex(0, -10) }, { new Complex(0, -10), new Complex(0, 200) } });
            var constantCurrents = new DenseVector(new[] { new Complex(1, 0), new Complex(0, -2) });
            var voltages = new DenseVector(new[] { new Complex(0.5, 0), new Complex(0.25, 0.1) });
            var rows = new List<int>() { 0, 1 };
            var columns = new List<int>() { 0, 1 };

            var changeMatrix = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(2, 2);
            JacobiMatrixBasedMethod.CalculateChangeMatrixImaginaryPowerByAmplitude(changeMatrix, admittances, voltages, constantCurrents, 0, 0, rows, columns);

            var v1 = voltages[0].Magnitude;
            var d1 = voltages[0].Phase;
            var v2 = voltages[1].Magnitude;
            var d2 = voltages[1].Phase;
            var leftUpper = (-200) * v1 + 10 * v2 * Math.Cos(d2 - d1) - Math.Sin(d1);
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
    }
}
