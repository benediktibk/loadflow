using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.SingleVoltageLevel;
using Calculation.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationTest.SinglePhase.SingleVoltageLevel.NodeVoltageCalculators
{
    [TestClass]
    public abstract class NodeVoltageCalculatorTest
    {
        private INodeVoltageCalculator _nodeVoltageCalculator;
        private AdmittanceMatrix _admittanceMatrix;
        private Vector<Complex> _constantCurrents;
        private Complex _feedInVoltage;
        private Complex _loadVoltageOne;
        private Complex _loadVoltageTwo;
        private DenseVector _knownVoltages;
        private DenseVector _initialVoltages;
        private DenseVector _correctVoltages;
        private Vector<Complex> _correctPowers;
        private List<int> _indexOfNodesWithKnownVoltage;
        private List<int> _indexOfNodesWithUnknownVoltage;
        private IReadOnlyAdmittanceMatrix _admittanceMatrixReduced;

        public abstract double PrecisionPqOnly { get; }
        public abstract double PrecisionPvOnly { get; }
        public abstract double PrecisionPqAndPv { get; }

        [TestInitialize]
        public void SetUp()
        {
            _nodeVoltageCalculator = CreateNodeVoltageCalculator();
            _admittanceMatrix = new AdmittanceMatrix(3);
            _admittanceMatrix.AddConnection(0, 1, 1 / new Complex(2, 1));
            _admittanceMatrix.AddConnection(1, 2, 1 / new Complex(1, 2));
            _feedInVoltage = new Complex(10, 0.1);
            _loadVoltageOne = new Complex(9.98, 0.09);
            _loadVoltageTwo = new Complex(9.96, 0.07);
            _knownVoltages = new DenseVector(new[] { _feedInVoltage });
            _initialVoltages = new DenseVector(new[] { new Complex(10, 0), new Complex(10, 0) });
            _correctVoltages = new DenseVector(new[] { _feedInVoltage, _loadVoltageOne, _loadVoltageTwo });
            _correctPowers = PowerNetComputable.CalculateAllPowers(_admittanceMatrix, _correctVoltages);
            _indexOfNodesWithKnownVoltage = new List<int> { 0 };
            _indexOfNodesWithUnknownVoltage = new List<int> { 1, 2 };
            _admittanceMatrixReduced = _admittanceMatrix.CreateReducedAdmittanceMatrix(_indexOfNodesWithUnknownVoltage, _indexOfNodesWithKnownVoltage, _knownVoltages, out _constantCurrents);
        }

        [TestMethod]
        public void CalculateUnknownVoltages_TwoPqBuses_CorrectResults()
        {
            var pqBuses = new List<PqBus> {new PqBus(0, _correctPowers[1]), new PqBus(1, _correctPowers[2])};
            var pvBuses = new List<PvBus>();
            
            var result = _nodeVoltageCalculator.CalculateUnknownVoltages(_admittanceMatrixReduced,
                _admittanceMatrix.CalculateRowSums(), 10, _initialVoltages, _constantCurrents, pqBuses, pvBuses);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(_loadVoltageOne.Magnitude, result[0].Magnitude, PrecisionPqOnly);
            Assert.AreEqual(_loadVoltageTwo.Magnitude, result[1].Magnitude, PrecisionPqOnly);
        }

        [TestMethod]
        public void CalculateUnknownVoltages_TwoPvBuses_CorrectResults()
        {
            var pqBuses = new List<PqBus>();
            var pvBuses = new List<PvBus> { new PvBus(0, _correctPowers[1].Real, _loadVoltageTwo.Magnitude), new PvBus(1, _correctPowers[2].Real, _loadVoltageTwo.Magnitude) };

            var result = _nodeVoltageCalculator.CalculateUnknownVoltages(_admittanceMatrixReduced,
                _admittanceMatrix.CalculateRowSums(), 10, _initialVoltages, _constantCurrents, pqBuses, pvBuses);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(_loadVoltageOne.Magnitude, result[0].Magnitude, PrecisionPvOnly);
            Assert.AreEqual(_loadVoltageTwo.Magnitude, result[1].Magnitude, PrecisionPvOnly);
        }

        [TestMethod]
        public void CalculateUnknownVoltages_OnePvAndOnePqBus_CorrectResults()
        {
            var pqBuses = new List<PqBus> { new PqBus(0, _correctPowers[1]) };
            var pvBuses = new List<PvBus> { new PvBus(1, _correctPowers[2].Real, _loadVoltageTwo.Magnitude) };

            var result = _nodeVoltageCalculator.CalculateUnknownVoltages(_admittanceMatrixReduced,
                _admittanceMatrix.CalculateRowSums(), 10, _initialVoltages, _constantCurrents, pqBuses, pvBuses);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(_loadVoltageOne.Magnitude, result[0].Magnitude, PrecisionPqAndPv);
            Assert.AreEqual(_loadVoltageTwo.Magnitude, result[1].Magnitude, PrecisionPqAndPv);
        }

        public abstract INodeVoltageCalculator CreateNodeVoltageCalculator();
    }
}
