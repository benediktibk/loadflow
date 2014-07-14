using System;
using System.Collections.Generic;
using System.Linq;
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

        private MathNet.Numerics.LinearAlgebra.Double.Matrix GetRealValues(AdmittanceMatrix matrix)
        {
            var rows = new int[matrix.NodeCount];
            var columns = new int[matrix.NodeCount];

            for (var i = 0; i < matrix.NodeCount; ++i)
            {
                rows[i] = i;
                columns[i] = i;
            }

            return GetRealValues(matrix, rows, columns);
        }

        private MathNet.Numerics.LinearAlgebra.Double.Matrix GetRealValues(AdmittanceMatrix matrix, IReadOnlyList<int> rows, IReadOnlyList<int> columns)
        {
            var values = matrix.GetValues();
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(rows.Count, columns.Count);

            for (var i = 0; i < rows.Count; ++i)
                for (var j = 0; j < columns.Count; ++j)
                    result[i, j] = values[rows[i], columns[j]].Real;

            return result;
        }

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
            ComplexAssert.AreEqual(0.5, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(-1, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0.5, 0, currents[2], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf10_CurrentsAreCorrect()
        {
            var input = new Node("input", 1);
            var output = new Node("output", 1);
            var ground = new Node("ground", 1);
            var internalNode = new Node("internalNode", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(4, nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 10, 1);
            matrix.AddConnection(output, ground, 1);

            var values = matrix.GetValues();
            var voltages =
                new DenseVector(new[] { new Complex(20, 0), new Complex(2, 0), new Complex(2, 0), new Complex(0, 0) });
            var currents = values * voltages;
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0.2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-0.2, 0, currents[3], 0.00001); 
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf10AndResistanceWeightOf100_CurrentsAreCorrect()
        {
            var input = new Node("input", 1);
            var output = new Node("output", 1);
            var ground = new Node("ground", 1);
            var internalNode = new Node("internalNode", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(4, nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 10, 100);
            matrix.AddConnection(output, ground, 1);

            var values = matrix.GetValues();
            var voltages =
                new DenseVector(new[] { new Complex(20, 0), new Complex(2, 0), new Complex(200, 0), new Complex(0, 0) });
            var currents = values * voltages;
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0.2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-0.2, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf1AndResistanceWeightOf100_CurrentsAreCorrect()
        {
            var input = new Node("input", 1);
            var output = new Node("output", 1);
            var ground = new Node("ground", 1);
            var internalNode = new Node("internalNode", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(4, nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 1, 100);
            matrix.AddConnection(output, ground, 1);

            var values = matrix.GetValues();
            var voltages =
                new DenseVector(new[] { new Complex(2, 0), new Complex(2, 0), new Complex(200, 0), new Complex(0, 0) });
            var currents = values * voltages;
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-2, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_OneConnectionBeforeAndAfter_CurrentsAreCorrect()
        {
            var input = new Node("input", 1);
            var inputTransformer = new Node("inputTransformer", 1);
            var output = new Node("output", 1);
            var outputTransformer = new Node("outputTransformer", 1);
            var ground = new Node("ground", 1);
            var internalNode = new Node("internalNode", 1);
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {inputTransformer, 1},
                {outputTransformer, 2},
                {output, 3},
                {internalNode, 4},
                {ground, 5}
            };
            var matrix = new AdmittanceMatrix(6, nodeIndexes);

            matrix.AddConnection(input, inputTransformer, 0.1);
            matrix.AddIdealTransformer(inputTransformer, ground, outputTransformer, ground, internalNode, 2, 1);
            matrix.AddConnection(outputTransformer, output, 0.1);

            var values = GetRealValues(matrix);
            var voltages =
                new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 35, 30, 15, 5, 1, 0 });
            var currents = values * voltages;
            Assert.AreEqual(6, currents.Count);
            Assert.AreEqual(0.5, currents[0], 0.00001);
            Assert.AreEqual(0, currents[1], 0.00001);
            Assert.AreEqual(0, currents[2], 0.00001);
            Assert.AreEqual(-1, currents[3], 0.00001);
            Assert.AreEqual(0, currents[4], 0.00001);
        }
    }
}
