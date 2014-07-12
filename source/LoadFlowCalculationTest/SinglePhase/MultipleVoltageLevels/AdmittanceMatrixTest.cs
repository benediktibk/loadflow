using System.Collections.Generic;
using System.Numerics;
using LoadFlowCalculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestHelper;

namespace LoadFlowCalculationTest.SinglePhase.MultipleVoltageLevels
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        private AdmittanceMatrix _matrix;
        private IReadOnlyDictionary<IReadOnlyNode, int> _nodeIndexes;
        private IReadOnlyNode _firstNode;
        private IReadOnlyNode _secondNode;
        private IReadOnlyNode _thirdNode;

        [TestInitialize]
        public void SetUp()
        {
            _firstNode = new Node("first", 1);
            _secondNode = new Node("second", 1);
            _thirdNode = new Node("third", 1);
            _nodeIndexes = new Dictionary<IReadOnlyNode, int>()
            {
                {_firstNode, 0},
                {_secondNode, 1},
                {_thirdNode, 2},
            };
            _matrix = new AdmittanceMatrix(3, _nodeIndexes);
        }

        [TestMethod]
        public void AddConnection_OnceCalled_CorrectValues()
        {
            _matrix.AddConnection(_firstNode, _thirdNode, new Complex(1, 2));

            var values = _matrix.GetValues();
            ComplexAssert.AreEqual(1, 2, values[0, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[0, 1], 0.00001);
            ComplexAssert.AreEqual(-1, -2, values[0, 2], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 1], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[1, 2], 0.00001);
            ComplexAssert.AreEqual(-1, -2, values[2, 0], 0.00001);
            ComplexAssert.AreEqual(0, 0, values[2, 1], 0.00001);
            ComplexAssert.AreEqual(1, 2, values[2, 2], 0.00001);
        }

        [TestMethod]
        public void AddVoltageControlledCurrentSource_AmplificationOf2_CurrentsAreCorrect()
        {
            var firstNode = new Node("first", 1);
            var secondNode = new Node("second", 1);
            var thirdNode = new Node("third", 1);
            var fourthNode = new Node("fourth", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {firstNode, 0},
                {secondNode, 1},
                {thirdNode, 2},
                {fourthNode, 3}
            };
            var matrix = new AdmittanceMatrix(4, nodeIndexes);

            matrix.AddVoltageControlledCurrentSource(firstNode, secondNode, thirdNode, fourthNode, 2);

            var values = matrix.GetValues();
            var voltages =
                new DenseVector(new[] {new Complex(4, 0), new Complex(1, 0), new Complex(6, 6), new Complex(10, 10)});
            var currents = values * voltages;
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(6, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-6, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddConnection_1_CurrentsAreCorrect()
        {
            var firstNode = new Node("first", 1);
            var secondNode = new Node("second", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {firstNode, 0},
                {secondNode, 1},
            };
            var matrix = new AdmittanceMatrix(2, nodeIndexes);

            matrix.AddConnection(firstNode, secondNode, new Complex(1, 0));

            var values = matrix.GetValues();
            var voltages =
                new DenseVector(new[] { new Complex(2, 0), new Complex(1, 0) });
            var currents = values * voltages;
            Assert.AreEqual(2, currents.Count);
            ComplexAssert.AreEqual(1, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(-1, 0, currents[1], 0.00001);
        }

        [TestMethod]
        public void AddGyrator_AmplificationOf2_CurrentsAreCorrect()
        {
            _matrix.AddGyrator(_firstNode, _thirdNode, _secondNode, _thirdNode, 2);

            var values = _matrix.GetValues();
            var voltages =
                new DenseVector(new[] { new Complex(2, 1), new Complex(1, 1), new Complex(0, 1) });
            var currents = values * voltages;
            Assert.AreEqual(3, currents.Count);
            ComplexAssert.AreEqual(2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(-4, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(2, 0, currents[2], 0.00001);
        }
    }
}
