using System.Collections.Generic;
using System.Numerics;
using Calculation.SinglePhase.MultipleVoltageLevels;
using MathNet.Numerics.LinearAlgebra.Complex;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Misc;

namespace CalculationIntegrationTest.SinglePhase.MultipleVoltageLevel
{
    [TestClass]
    public class AdmittanceMatrixTest
    {
        [TestMethod]
        public void AddVoltageControlledCurrentSource_AmplificationOf2_CurrentsAreCorrect()
        {
            var firstNode = new Node(0, 1, 0, "");
            var secondNode = new Node(1, 1, 0, "");
            var thirdNode = new Node(2, 1, 0, "");
            var fourthNode = new Node(3, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {firstNode, 0},
                {secondNode, 1},
                {thirdNode, 2},
                {fourthNode, 3}
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(4), nodeIndexes);

            matrix.AddVoltageControlledCurrentSource(firstNode, secondNode, thirdNode, fourthNode, 2);

            var voltages =
                new DenseVector(new[] { new Complex(4, 0), new Complex(1, 0), new Complex(6, 6), new Complex(10, 10) });
            var currents = matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(6, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-6, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddConnection_1_CurrentsAreCorrect()
        {
            var firstNode = new Node(0, 1, 0, "");
            var secondNode = new Node(1, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {firstNode, 0},
                {secondNode, 1},
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(2), nodeIndexes);

            matrix.AddConnection(firstNode, secondNode, new Complex(1, 0));

            var voltages =
                new DenseVector(new[] { new Complex(2, 0), new Complex(1, 0) });
            var currents = matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(2, currents.Count);
            ComplexAssert.AreEqual(1, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(-1, 0, currents[1], 0.00001);
        }

        [TestMethod]
        public void AddGyrator_AmplificationOf2_CurrentsAreCorrect()
        {
            var firstNode = new Node(0, 1, 0, "");
            var secondNode = new Node(1, 1, 0, "");
            var thirdNode = new Node(2, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>()
            {
                {firstNode, 0},
                {secondNode, 1},
                {thirdNode, 2},
            };
            var admittanceMatrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(3), nodeIndexes);
            admittanceMatrix.AddGyrator(firstNode, thirdNode, secondNode, thirdNode, 2);

            var voltages =
                new DenseVector(new[] { new Complex(2, 1), new Complex(1, 1), new Complex(0, 1) });
            var currents = admittanceMatrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(3, currents.Count);
            ComplexAssert.AreEqual(0.5, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(-1, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0.5, 0, currents[2], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf10_CurrentsAreCorrect()
        {
            var input = new Node(0, 1, 0, "");
            var output = new Node(1, 1, 0, "");
            var ground = new Node(-1, 1, 0, "");
            var internalNode = new Node(-2, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(4), nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 10, 1);
            matrix.AddConnection(output, ground, 1);

            var voltages =
                new DenseVector(new[] { new Complex(20, 0), new Complex(2, 0), new Complex(2, 0), new Complex(0, 0) });
            var currents = matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0.2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-0.2, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf10AndResistanceWeightOf100_CurrentsAreCorrect()
        {
            var input = new Node(0, 1, 0, "");
            var output = new Node(1, 1, 0, "");
            var ground = new Node(-1, 1, 0, "");
            var internalNode = new Node(-2, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(4), nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 10, 100);
            matrix.AddConnection(output, ground, 1);

            var voltages =
                new DenseVector(new[] { new Complex(20, 0), new Complex(2, 0), new Complex(200, 0), new Complex(0, 0) });
            var currents = matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(0.2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-0.2, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_AmplificationOf1AndResistanceWeightOf100_CurrentsAreCorrect()
        {
            var input = new Node(0, 1, 0, "");
            var output = new Node(1, 1, 0, "");
            var ground = new Node(-1, 1, 0, "");
            var internalNode = new Node(-2, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {output, 1},
                {internalNode, 2},
                {ground, 3}
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(4), nodeIndexes);

            matrix.AddIdealTransformer(input, ground, output, ground, internalNode, 1, 100);
            matrix.AddConnection(output, ground, 1);

            var voltages =
                new DenseVector(new[] { new Complex(2, 0), new Complex(2, 0), new Complex(200, 0), new Complex(0, 0) });
            var currents = matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages);
            Assert.AreEqual(4, currents.Count);
            ComplexAssert.AreEqual(2, 0, currents[0], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[1], 0.00001);
            ComplexAssert.AreEqual(0, 0, currents[2], 0.00001);
            ComplexAssert.AreEqual(-2, 0, currents[3], 0.00001);
        }

        [TestMethod]
        public void AddIdealTransformer_OneConnectionBeforeAndAfter_CurrentsAreCorrect()
        {
            var input = new Node(0, 1, 0, "");
            var inputTransformer = new Node(10, 1, 0, "");
            var output = new Node(1, 1, 0, "");
            var outputTransformer = new Node(11, 1, 0, "");
            var ground = new Node(-1, 1, 0, "");
            var internalNode = new Node(-2, 1, 0, "");
            var nodeIndexes = new Dictionary<IReadOnlyNode, int>
            {
                {input, 0},
                {inputTransformer, 1},
                {outputTransformer, 2},
                {output, 3},
                {internalNode, 4},
                {ground, 5}
            };
            var matrix = new AdmittanceMatrix(new Calculation.SinglePhase.SingleVoltageLevel.AdmittanceMatrix(6), nodeIndexes);

            matrix.AddConnection(input, inputTransformer, 0.1);
            matrix.AddIdealTransformer(inputTransformer, ground, outputTransformer, ground, internalNode, 2, 1);
            matrix.AddConnection(outputTransformer, output, 0.1);

            var voltages = new DenseVector(new[] { new Complex(35, 0), new Complex(30, 0), new Complex(15, 0), new Complex(5, 0), new Complex(1, 0), new Complex(0, 0) });
            var currents = GetRealValues(matrix.SingleVoltageAdmittanceMatrix.CalculateCurrents(voltages));
            Assert.AreEqual(6, currents.Count);
            Assert.AreEqual(0.5, currents[0], 0.00001);
            Assert.AreEqual(0, currents[1], 0.00001);
            Assert.AreEqual(0, currents[2], 0.00001);
            Assert.AreEqual(-1, currents[3], 0.00001);
            Assert.AreEqual(0, currents[4], 0.00001);
            Assert.AreEqual(0.5, currents[5], 0.00001);
        }

        private static MathNet.Numerics.LinearAlgebra.Double.Vector GetRealValues(IList<Complex> vector)
        {
            var result = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(vector.Count);

            for (var i = 0; i < vector.Count; ++i)
                result[i] = vector[i].Real;

            return result;
        }
    }
}
