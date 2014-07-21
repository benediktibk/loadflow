using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.SingleVoltageLevel;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.SingleVoltageLevel
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
    }
}
