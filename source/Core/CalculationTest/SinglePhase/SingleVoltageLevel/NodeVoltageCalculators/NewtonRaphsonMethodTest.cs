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
    public class NewtonRaphsonMethodTest : NodeVoltageCalculatorTest
    {
        public override double PrecisionPqOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPvOnly
        {
            get { return 0.0001; }
        }

        public override double PrecisionPqAndPv
        {
            get { return 0.001; }
        }

        public override INodeVoltageCalculator CreateNodeVoltageCalculator()
        {
            return new NewtonRaphsonMethod(0.0000001, 1000);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqNodes_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double> { 19, 20 };
            var pqBuses = new List<int> { 0, 1 };
            var pvBuses = new List<int>();
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(10.7080142194379, 8.72599840262781, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(12.9855590454174, 10.2313627457633, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqNodesInverseOrder_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double> { 19, 20 };
            var pqBuses = new List<int> { 1, 0 };
            var pvBuses = new List<int>();
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(10.7188134641008, 8.71539079667836, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(13.0136419813066, 10.2082477701455, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPvNodes_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new List<int>();
            var pvBuses = new List<int> { 0, 1 };
            var pvBusVoltages = new List<double> { 21, 22 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(21, 5.1869348191849, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(22, 5.17825761201918, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPvNodesInverseOrder_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4)},
                {new Complex(5, 6), new Complex(7, 8)}
            }));
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var constantCurrents = new DenseVector(new[] { new Complex(13, 14), new Complex(15, 16) });
            var powersRealError = new List<double> { 17, 18 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new List<int>();
            var pvBuses = new List<int> { 1, 0 };
            var pvBusVoltages = new List<double> { 21, 22 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(2, improvedVoltages.Count);
            ComplexAssert.AreEqual(22, 5.2390276308319, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(21, 5.22944050555881, improvedVoltages[1], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqAndOnePvNode_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
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
            var pqBuses = new List<int> { 0, 2 };
            var pvBuses = new List<int> { 1 };
            var pvBusVoltages = new List<double> { 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(20.622334959047, 18.6013743971205, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(36, 0.731296048182103, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(24.8767053116905, 22.2064947617015, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_TwoPqAndOnePvNodeDifferentOrder_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
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
            var pqBuses = new List<int> { 0, 1 };
            var pvBuses = new List<int> { 2 };
            var pvBusVoltages = new List<double> { 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(20.3970135969122, 18.7885158385366, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(22.5007569023519, 20.5914026073495, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(36, 0.738925723134326, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateChangeMatrix_TwoPqAndOnePvNodeDifferentOrder_CorrectResults()
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var pqBuses = new List<int> { 0, 1 };
            var pvBuses = new List<int> { 2 };

            var changeMatrix = NewtonRaphsonMethod.CalculateChangeMatrix(admittances, voltages, constantCurrents,
                pqBuses, pvBuses);

            Assert.AreEqual(5, changeMatrix.RowCount);
            Assert.AreEqual(5, changeMatrix.ColumnCount);
            Assert.AreEqual(-41, changeMatrix[0, 0], 0.00001);
            Assert.AreEqual(323, changeMatrix[1, 0], 0.00001);
            Assert.AreEqual(635, changeMatrix[2, 0], 0.00001);
            Assert.AreEqual(-458, changeMatrix[3, 0], 0.00001);
            Assert.AreEqual(-14, changeMatrix[4, 0], 0.00001);
            Assert.AreEqual(137, changeMatrix[0, 1], 0.00001);
            Assert.AreEqual(289, changeMatrix[1, 1], 0.00001);
            Assert.AreEqual(729, changeMatrix[2, 1], 0.00001);
            Assert.AreEqual(-16, changeMatrix[3, 1], 0.00001);
            Assert.AreEqual(-1224, changeMatrix[4, 1], 0.00001);
            Assert.AreEqual(422, changeMatrix[0, 2], 0.00001);
            Assert.AreEqual(-14, changeMatrix[1, 2], 0.00001);
            Assert.AreEqual(-10, changeMatrix[2, 2], 0.00001);
            Assert.AreEqual(-159, changeMatrix[3, 2], 0.00001);
            Assert.AreEqual(-323, changeMatrix[4, 2], 0.00001);
            Assert.AreEqual(-16, changeMatrix[0, 3], 0.00001);
            Assert.AreEqual(1200, changeMatrix[1, 3], 0.00001);
            Assert.AreEqual(-8, changeMatrix[2, 3], 0.00001);
            Assert.AreEqual(-137, changeMatrix[3, 3], 0.00001);
            Assert.AreEqual(-529, changeMatrix[4, 3], 0.00001);
            Assert.AreEqual(-5482, changeMatrix[0, 4], 0.00001);
            Assert.AreEqual(-12110, changeMatrix[1, 4], 0.00001);
            Assert.AreEqual(29102, changeMatrix[2, 4], 0.00001);
            Assert.AreEqual(-4609, changeMatrix[3, 4], 0.00001);
            Assert.AreEqual(-11145, changeMatrix[4, 4], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrix_OnePqAndOnePvNode_CorrectResults()
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(0.6, -0.6), new Complex(-0.2, 0.4)},
                {new Complex(-0.2, 0.4), new Complex(0.2, -0.4)}
            }));
            var voltages = new DenseVector(new[] { new Complex(10, 0), new Complex(10, 0) });
            var constantCurrents = new DenseVector(new[] { new Complex(4.02, -1.96), new Complex(0, 0) });
            var pqBuses = new List<int> { 0 };
            var pvBuses = new List<int> { 1 };

            var changeMatrix = NewtonRaphsonMethod.CalculateChangeMatrix(admittances, voltages, constantCurrents,
                pqBuses, pvBuses);

            Assert.AreEqual(3, changeMatrix.RowCount);
            Assert.AreEqual(3, changeMatrix.ColumnCount);
            Assert.AreEqual(5.98, changeMatrix[0, 0], 0.00001);
            Assert.AreEqual(-2, changeMatrix[1, 0], 0.00001);
            Assert.AreEqual(6.04, changeMatrix[2, 0], 0.00001);
            Assert.AreEqual(5.96, changeMatrix[0, 1], 0.00001);
            Assert.AreEqual(-4, changeMatrix[1, 1], 0.00001);
            Assert.AreEqual(-6.02, changeMatrix[2, 1], 0.00001);
            Assert.AreEqual(-40, changeMatrix[0, 2], 0.00001);
            Assert.AreEqual(40, changeMatrix[1, 2], 0.00001);
            Assert.AreEqual(20, changeMatrix[2, 2], 0.00001);
        }

        [TestMethod]
        public void CalculateChangeMatrix_OnePqAndOnePvNode_SparseMatrix()
        {
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(0.6, -0.6), new Complex(-0.2, 0.4)},
                {new Complex(-0.2, 0.4), new Complex(0.2, -0.4)}
            }));
            var voltages = new DenseVector(new[] { new Complex(10, 0), new Complex(10, 0) });
            var constantCurrents = new DenseVector(new[] { new Complex(4.02, -1.96), new Complex(0, 0) });
            var pqBuses = new List<int> { 0 };
            var pvBuses = new List<int> { 1 };

            var changeMatrix = NewtonRaphsonMethod.CalculateChangeMatrix(admittances, voltages, constantCurrents,
                pqBuses, pvBuses);

            Assert.IsNotNull(changeMatrix as MathNet.Numerics.LinearAlgebra.Double.SparseMatrix);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_ThreePqNodes_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
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
            var pqBuses = new List<int> { 0, 1, 2 };
            var pvBuses = new List<int>();
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(20.400950369329, 18.7852777611224, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(22.5048378956587, 20.587655826364, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(24.6298557212699, 22.437680614831, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_ThreePqNodesInverseOrder_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
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
            var pqBuses = new List<int> { 2, 1, 0 };
            var pvBuses = new List<int>();
            var pvBusVoltages = new List<double>();

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(20.4252671775033, 18.7635714441913, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(22.5349039278552, 20.5596401320307, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(24.6630992108621, 22.4060801078657, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_ThreePvNodes_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var powersRealError = new List<double> { 31, 32, 33 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new List<int>();
            var pvBuses = new List<int> { 0, 1, 2 };
            var pvBusVoltages = new List<double> { 34, 35, 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(34, 6.09350183532044, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(35, 6.0911881675974, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(36, 6.08922370817489, improvedVoltages[2], 0.000001);
        }

        [TestMethod]
        public void CalculateImprovedVoltages_ThreePvNodesInverseOrder_CorrectResults()
        {
            var calculator = new NewtonRaphsonMethod(0.000001, 1000);
            var admittances = new AdmittanceMatrix(DenseMatrix.OfArray(new[,]
            {
                {new Complex(1, 2), new Complex(3, 4), new Complex(5, 6)},
                {new Complex(7, 8), new Complex(9, 10), new Complex(11, 12)},
                {new Complex(13, 14), new Complex(15, 16), new Complex(17, 18)}
            }));
            var voltages = new DenseVector(new[] { new Complex(19, 20), new Complex(21, 22), new Complex(23, 24) });
            var constantCurrents = new DenseVector(new[] { new Complex(25, 26), new Complex(27, 28), new Complex(29, 30) });
            var powersRealError = new List<double> { 31, 32, 33 };
            var powersImaginaryError = new List<double>();
            var pqBuses = new List<int>();
            var pvBuses = new List<int> { 2, 1, 0 };
            var pvBusVoltages = new List<double> { 34, 35, 36 };

            var improvedVoltages = calculator.CalculateImprovedVoltages(admittances, voltages, constantCurrents, powersRealError,
                powersImaginaryError, pqBuses, pvBuses, pvBusVoltages, 1e-12);

            Assert.AreEqual(3, improvedVoltages.Count);
            ComplexAssert.AreEqual(36, 6.19523197518303, improvedVoltages[0], 0.000001);
            ComplexAssert.AreEqual(35, 6.19277807148365, improvedVoltages[1], 0.000001);
            ComplexAssert.AreEqual(34, 6.19078605733335, improvedVoltages[2], 0.000001);
        }
    }
}
