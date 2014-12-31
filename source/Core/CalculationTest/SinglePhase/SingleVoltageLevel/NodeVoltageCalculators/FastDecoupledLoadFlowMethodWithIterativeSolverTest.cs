using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public class FastDecoupledLoadFlowMethodWithIterativeSolverTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 10; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 10; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new FastDecoupledLoadFlowMethod(0.0000001, 1000, true);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqNodes_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double> { 19, 20 };
            var pqBuses = new Dictionary<int, int> { {0, 0}, {1, 1} };
            var pvBuses = new Dictionary<int, int>();
            var buses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(6.07727309100956, -11.8304401215607, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(7.30600714771135, -14.531238934229, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqNodesInverseOrder_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double> { 19, 20 };
            var pqBuses = new Dictionary<int, int> { { 1, 0 }, { 0, 1 } };
            var pvBuses = new Dictionary<int, int>();
            var buses = new Dictionary<int, int> { { 1, 0 }, { 0, 1 } };
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(6.6741834507365, -11.4913234639654, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(8.0493352781657, -14.1401076704863, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPvNodes_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new Dictionary<int, int>();
            var pvBuses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
            var buses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
            var pvBusVoltages = new List<double> { 21, 22 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(9.59562487332882, -18.6795070408818, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(9.88237874499239, -19.6554977128671, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPvNodesInverseOrder_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new Dictionary<int, int>();
            var pvBuses = new Dictionary<int, int> { { 1, 0 }, { 0, 1 } };
            var buses = new Dictionary<int, int> { { 1, 0 }, { 0, 1 } };
            var pvBusVoltages = new List<double> { 21, 22 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(11.0492124557286, -19.0240611886151, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(10.3890081659455, -18.2501646383784, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqAndOnePvNode_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var powersRealError = new List<double> { 31, 32, 33 };
            var powersImaginaryError = new List<double> { 34, 35 };
            var pqBuses = new Dictionary<int, int> { { 0, 0 }, { 2, 1 } };
            var pvBuses = new Dictionary<int, int> { { 1, 0 } };
            var buses = new Dictionary<int, int> { { 0, 0 }, { 2, 1 }, { 1, 2 } };
            var pvBusVoltages = new List<double> { 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(27.0659000440881, -4.90582939841207 , improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(35.4078734444971, -6.50249937627648, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(32.68406960966, -6.06776444261958, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqAndOnePvNodeDifferentOrder_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var powersRealError = new List<double> { 31, 32, 33 };
            var powersImaginaryError = new List<double> { 34, 35 };
            var pqBuses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 } };
            var pvBuses = new Dictionary<int, int> { { 2, 0 } };
            var buses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } };
            var pvBusVoltages = new List<double> { 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(27.0173142661381, -5.1870979875702, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(29.841692902066, -5.80097573381533, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(35.3249405264103, -6.93891755287353, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_ThreePqNodes_CorrectResults()
        {
            var calculator = new FastDecoupledLoadFlowMethod(0.000001, 1000, true);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var powersRealError = new List<double> { 31, 32, 33 };
            var powersImaginaryError = new List<double> { 34, 35, 36 };
            var pqBuses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } };
            var pvBuses = new Dictionary<int, int>();
            var buses = new Dictionary<int, int> { { 0, 0 }, { 1, 1 }, { 2, 2 } };
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pvBusVoltages, 1e-12, pqBuses, pvBuses, buses);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(27.0161704499591, -5.18687838444674, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(29.8408710634224, -5.80081597525055, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(32.621740115625, -6.4079248746181, improvedVoltages[2], 0.000001);
        }
    }
}
