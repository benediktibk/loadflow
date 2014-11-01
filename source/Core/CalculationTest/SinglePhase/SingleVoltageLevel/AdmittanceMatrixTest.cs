using System;
using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

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
    }
}
