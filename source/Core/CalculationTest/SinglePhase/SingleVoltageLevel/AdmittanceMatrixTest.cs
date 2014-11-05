using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;
using Moq;

namespace CalculationTest.SinglePhase.SingleVoltageLevel
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        private AdmittanceMatrix _admittances;
        private List<int> _knownVoltageIndex;
        private List<int> _unknownVoltageIndex;
        private Vector<Complex> _knownVoltages;
        private Vector<Complex> _unknownVoltages;
        private Vector<Complex> _constantCurrents;

        [TestInitialize]
        public void SetUp()
        {
            _admittances = null;
            _knownVoltageIndex = new List<int>();
            _unknownVoltageIndex = new List<int>();
            _knownVoltages = null;
            _unknownVoltages = null;
            _constantCurrents = null;
        }

        [TestMethod]
        public void CreateReducedAdmittanceMatrix_OneIdealTransformerWithTwoConnectionsAndInputVoltageKnown_CurrentsAreCorrect()
        {
            _admittances = new AdmittanceMatrix(6);
            _admittances.AddConnection(1, 2, new Complex(0.1, 0));
            _admittances.AddConnection(4, 5, new Complex(0.1, 0));
            _admittances.AddIdealTransformer(2, 0, 4, 0, 3, 2, 1);
            _knownVoltageIndex.Add(0);
            _knownVoltageIndex.Add(1);
            _unknownVoltageIndex.Add(2);
            _unknownVoltageIndex.Add(3);
            _unknownVoltageIndex.Add(4);
            _unknownVoltageIndex.Add(5);
            _knownVoltages = new DenseVector(new [] { new Complex(0, 0), new Complex(35, 0) });

            var reducedMatrix = _admittances.CreateReducedAdmittanceMatrix(_unknownVoltageIndex, _knownVoltageIndex,
                _knownVoltages, out _constantCurrents);
    
            _unknownVoltages = new DenseVector(new [] { new Complex(30, 0), new Complex(1, 0), new Complex(15, 0), new Complex(5, 0) });
            var branchCurrents = reducedMatrix.CalculateCurrents(_unknownVoltages);
            var totalCurrents = branchCurrents - _constantCurrents;
            ComplexAssert.AreEqual(0, 0, totalCurrents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, totalCurrents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, totalCurrents[2], 0.00001);
            ComplexAssert.AreEqual(-1, 0, totalCurrents[3], 0.00001);
        }

        [TestMethod]
        public void CreateReducedAdmittanceMatrix_OneIdealTransformerWithTwoConnectionsAndInputVoltageKnownAndNoLoad_CurrentsAreCorrect()
        {
            _admittances = new AdmittanceMatrix(6);
            _admittances.AddConnection(1, 2, new Complex(0.1, 0));
            _admittances.AddConnection(4, 5, new Complex(0.1, 0));
            _admittances.AddIdealTransformer(2, 0, 4, 0, 3, 2, 1);
            _knownVoltageIndex.Add(0);
            _knownVoltageIndex.Add(1);
            _unknownVoltageIndex.Add(2);
            _unknownVoltageIndex.Add(3);
            _unknownVoltageIndex.Add(4);
            _unknownVoltageIndex.Add(5);
            _knownVoltages = new DenseVector(new[] { new Complex(0, 0), new Complex(30, 0) });

            var reducedMatrix = _admittances.CreateReducedAdmittanceMatrix(_unknownVoltageIndex, _knownVoltageIndex,
                _knownVoltages, out _constantCurrents);

            _unknownVoltages = new DenseVector(new[] { new Complex(30, 0), new Complex(0, 0), new Complex(15, 0), new Complex(15, 0) });
            var branchCurrents = reducedMatrix.CalculateCurrents(_unknownVoltages);
            var totalCurrents = branchCurrents - _constantCurrents;
            ComplexAssert.AreEqual(0, 0, totalCurrents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, totalCurrents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, totalCurrents[2], 0.00001);
            ComplexAssert.AreEqual(0, 0, totalCurrents[3], 0.00001);
        }

        [TestMethod]
        public void AddCurrentControlledCurrentSource_ValidVoltages_CurrentsAreCorrect()
        {
            _admittances = new AdmittanceMatrix(5);

            _admittances.AddCurrentControlledCurrentSource(0, 1, 2, 3, 4, 3, 10);

            var voltages = new DenseVector(new[]
            {
                new Complex(5, 0), new Complex(5, 0), new Complex(7, 0), new Complex(3, 0), new Complex(25, 0)
            });
            var currents = _admittances.CalculateCurrents(voltages);
            ComplexAssert.AreEqual(2, 0, currents[0], 0.0001);
            ComplexAssert.AreEqual(-2, 0, currents[1], 0.0001);
            ComplexAssert.AreEqual(6, 0, currents[2], 0.0001);
            ComplexAssert.AreEqual(-6, 0, currents[3], 0.0001);
            ComplexAssert.AreEqual(0, 0, currents[4], 0.0001);
        }

        [TestMethod]
        public void AddVoltageControlledVoltageSource_ValidVoltages_CurrentsAreCorrect()
        {
            _admittances = new AdmittanceMatrix(5);

            _admittances.AddVoltageControlledVoltageSource(0, 1, 3, 4, 2, 4, 10);

            var voltages = new DenseVector(new[]
            {
                new Complex(12, 0), new Complex(9, 0), new Complex(-8, 0), new Complex(14, 0), new Complex(2, 0)
            });
            var currents = _admittances.CalculateCurrents(voltages);
            ComplexAssert.AreEqual(0, 0, currents[0], 0.0001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.0001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.0001);
            ComplexAssert.AreEqual(1, 0, currents[3], 0.0001);
            ComplexAssert.AreEqual(-1, 0, currents[4], 0.0001);
        }

        [TestMethod]
        public void AddIdealTransformer_RotationOnly_CurrentsAreCorrect()
        {
            _admittances = new AdmittanceMatrix(5);

            _admittances.AddIdealTransformer(0, 1, 2, 3, 4, Complex.FromPolarCoordinates(1, -0.5), 10);

            var voltages = new DenseVector(new[]
            {
                new Complex(12, 0), new Complex(10, 0), new Complex(11.75516512, -1.041148923), new Complex(10, -2), new Complex(20, -3)
            });
            var currents = _admittances.CalculateCurrents(voltages);
            var inputCurrent = currents[0];
            var outputCurrent = currents[2];
            var currentRatio = (-1)*inputCurrent/outputCurrent;
            ComplexAssert.AreEqual((-1) * inputCurrent, currents[1], 0.0001);
            ComplexAssert.AreEqual((-1) * outputCurrent, currents[3], 0.0001);
            ComplexAssert.AreEqual(Complex.FromPolarCoordinates(1, 0.5), currentRatio, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NegativeNodeCount_ThrowsException()
        {
            var admittanceMatrix = new AdmittanceMatrix(-5);
        }

        [TestMethod]
        public void AddConnection_OnceCalled_ValuesAreCorrect()
        {
            _admittances = new AdmittanceMatrix(2);

            _admittances.AddConnection(0, 1, new Complex(2, 3));

            ComplexAssert.AreEqual(2, 3, _admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(2, 3, _admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-2, -3, _admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-2, -3, _admittances[1, 0], 0.00001);
        }

        [TestMethod]
        public void AddConnection_TwiceCalled_ValuesAreCorrect()
        {
            _admittances = new AdmittanceMatrix(2);

            _admittances.AddConnection(0, 1, new Complex(2, 3));
            _admittances.AddConnection(0, 1, new Complex(4, 5));

            ComplexAssert.AreEqual(6, 8, _admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(6, 8, _admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-6, -8, _admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-6, -8, _admittances[1, 0], 0.00001);
        }

        [TestMethod]
        public void AddUnsymmetricAdmittance_PrefilledMatrix_ValuesAreCorrect()
        {
            _admittances = new AdmittanceMatrix(2);
            _admittances.AddConnection(0, 1, new Complex(2, 3));

            _admittances.AddUnsymmetricAdmittance(1, 0, new Complex(4, 5));

            ComplexAssert.AreEqual(2, 3, _admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(2, 3, _admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-2, -3, _admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(2, 2, _admittances[1, 0], 0.00001);
        }

        [TestMethod]
        public void AddVoltageControlledCurrentSource_PrefilledMatrix_ValuesAreCorrect()
        {
            _admittances = new AdmittanceMatrix(4);
            _admittances.AddConnection(0, 1, new Complex(2, 3));
            _admittances.AddConnection(2, 3, new Complex(4, 5));

            _admittances.AddVoltageControlledCurrentSource(0, 1, 2, 3, new Complex(6, 7));

            ComplexAssert.AreEqual(2, 3, _admittances[0, 0], 0.00001);
            ComplexAssert.AreEqual(2, 3, _admittances[1, 1], 0.00001);
            ComplexAssert.AreEqual(-2, -3, _admittances[0, 1], 0.00001);
            ComplexAssert.AreEqual(-2, -3, _admittances[1, 0], 0.00001);
            ComplexAssert.AreEqual(4, 5, _admittances[2, 2], 0.00001);
            ComplexAssert.AreEqual(4, 5, _admittances[3, 3], 0.00001);
            ComplexAssert.AreEqual(-4, -5, _admittances[2, 3], 0.00001);
            ComplexAssert.AreEqual(-4, -5, _admittances[3, 2], 0.00001);
            ComplexAssert.AreEqual(0, 0, _admittances[0, 2], 0.00001);
            ComplexAssert.AreEqual(0, 0, _admittances[0, 3], 0.00001);
            ComplexAssert.AreEqual(0, 0, _admittances[1, 2], 0.00001);
            ComplexAssert.AreEqual(0, 0, _admittances[1, 3], 0.00001);
            ComplexAssert.AreEqual(6, 7, _admittances[2, 0], 0.00001);
            ComplexAssert.AreEqual(-6, -7, _admittances[3, 0], 0.00001);
            ComplexAssert.AreEqual(-6, -7, _admittances[2, 1], 0.00001);
            ComplexAssert.AreEqual(6, 7, _admittances[3, 1], 0.00001);
        }

        [TestMethod]
        public void NodeCount_SetTo3_3()
        {
            _admittances = new AdmittanceMatrix(3);

            Assert.AreEqual(3, _admittances.NodeCount);
        }

        [TestMethod]
        public void GetRow_ValidRowNumber_CorrectValues()
        {
            _admittances = new AdmittanceMatrix(2);
            _admittances.AddConnection(0, 1, new Complex(2, 3));

            var row = _admittances.GetRow(1);

            ComplexAssert.AreEqual(-2, -3, row[0], 0.0001);
            ComplexAssert.AreEqual(2, 3, row[1], 0.0001);
        }

        [TestMethod]
        public void CalculateRowSums_UnsymmetricValues_CorrectResults()
        {
            _admittances = new AdmittanceMatrix(4);
            _admittances.AddConnection(0, 1, new Complex(2, 3));
            _admittances.AddConnection(2, 3, new Complex(4, 5));
            _admittances.AddVoltageControlledCurrentSource(0, 1, 2, 3, new Complex(6, 7));
            _admittances.AddUnsymmetricAdmittance(2, 3, new Complex(8, 9));
            _admittances.AddUnsymmetricAdmittance(3, 1, new Complex(10, 11));

            var rowSums = _admittances.CalculateRowSums();

            ComplexAssert.AreEqual(0, 0, rowSums[0], 0.000001);
            ComplexAssert.AreEqual(0, 0, rowSums[1], 0.000001);
            ComplexAssert.AreEqual(8, 9, rowSums[2], 0.000001);
            ComplexAssert.AreEqual(10, 11, rowSums[3], 0.000001);
        }

        [TestMethod]
        public void CalculateFactorization_OneConnection_SolverDeliversCorrectVoltages()
        {
            _admittances = new AdmittanceMatrix(2);
            _admittances.AddConnection(0, 1, new Complex(2, 3));
            _admittances.AddUnsymmetricAdmittance(1, 1, new Complex(8, 9));
            var correctVoltages = new DenseVector(new[] {new Complex(4, 5), new Complex(6, 7)});
            var currents = _admittances.CalculateCurrents(correctVoltages);

            var factorization = _admittances.CalculateFactorization();

            var voltages = factorization.Solve(currents);
            ComplexAssert.AreAllEqual(correctVoltages, voltages, 0.0001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_NotQuadraticMatrix_ThrowsException()
        {
            var matrix = new SparseMatrix(4, 5);

            var admittances = new AdmittanceMatrix(matrix);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public void AddIdealTransformer_ZeroRatio_ThrowsException()
        {
            _admittances = new AdmittanceMatrix(5);

            _admittances.AddIdealTransformer(0, 1, 2, 3, 4, new Complex(), 5);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddIdealTransformer_NegativeResistanceWeight_ThrowsException()
        {
            _admittances = new AdmittanceMatrix(5);

            _admittances.AddIdealTransformer(0, 1, 2, 3, 4, new Complex(1, 3), -4);
        }

        [TestMethod]
        public void CalculatePowerLoss_TwoNodeSystem_CorrectResult()
        {
            var admittances =
                new AdmittanceMatrix(DenseMatrix.OfArray(new[,] { { new Complex(1, 0), new Complex(-1, 0) }, { new Complex(-1, 0), new Complex(1, 0) } }));
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0.5, 0) });

            var powerLoss = admittances.CalculatePowerLoss(voltages);

            ComplexAssert.AreEqual(0.25, 0, powerLoss, 0.0001);
        }

        [TestMethod]
        public void CalculatePowerLoss_ThreeNodeSystem_CorrectResult()
        {
            var matrix = new DenseMatrix(3, 3);
            var voltages = new DenseVector(new[] { new Complex(1, 0), new Complex(0.5, 0), new Complex(0.25, 0) });
            matrix[0, 0] = new Complex(1 + 1.0 / 3, 0);
            matrix[0, 1] = new Complex(-1, 0);
            matrix[0, 2] = new Complex(-1.0 / 3, 0);
            matrix[1, 0] = new Complex(-1, 0);
            matrix[1, 1] = new Complex(1.5, 0);
            matrix[1, 2] = new Complex(-0.5, 0);
            matrix[2, 0] = new Complex(-1.0 / 3, 0);
            matrix[2, 1] = new Complex(-0.5, 0);
            matrix[2, 2] = new Complex(1.0 / 3 + 0.5, 0);
            _admittances = new AdmittanceMatrix(matrix);

            var powerLoss = _admittances.CalculatePowerLoss(voltages);

            ComplexAssert.AreEqual(0.46875, 0, powerLoss, 0.0000001);
        }

        [TestMethod]
        public void CalculateAllPowers_WithConstantCurrents_CorrectResults()
        {
            _admittances = new AdmittanceMatrix(2);
            _admittances.AddConnection(0, 1, new Complex(1, 2));
            var constantCurrents = new DenseVector(new[] { new Complex(1, 2), new Complex(3, 4) });
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });

            var powers = _admittances.CalculateAllPowers(voltages, constantCurrents);

            ComplexAssert.AreEqual(-71, 82, powers[0], 0.00001);
            ComplexAssert.AreEqual(-31, -82, powers[1], 0.00001);
        }

        [TestMethod]
        public void CalculatePowerError_OnePqAndOnePvBus_CorrectResult()
        {
            _admittances = new AdmittanceMatrix(2);
            _admittances.AddConnection(0, 1, new Complex(1, 2));
            var constantCurrents = new DenseVector(new[] { new Complex(1, 2), new Complex(3, 4) });
            var voltages = new DenseVector(new[] { new Complex(9, 10), new Complex(11, 12) });
            var powers = _admittances.CalculateAllPowers(voltages, constantCurrents);
            var pqBuses = new List<PqNodeWithIndex> { new PqNodeWithIndex(0, powers[0] - new Complex(0.1, 0.2)) };
            var pvBuses = new List<PvNodeWithIndex> { new PvNodeWithIndex(1, powers[1].Real - 0.3, 1) };

            var powerError = _admittances.CalculatePowerError(voltages,
                constantCurrents, pqBuses, pvBuses);

            Assert.AreEqual(0.6, powerError, 0.00001);
        }
    }
}
